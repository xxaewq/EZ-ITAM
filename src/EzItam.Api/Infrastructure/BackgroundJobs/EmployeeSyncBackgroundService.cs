using EzItam.Api.ExternalService;
using Hangfire;
using SQLitePCL;

namespace EzItam.Api.Infrastructure.BackgroundJobs
{
    public class EmployeeSyncBackgroundService(IServiceProvider serviceProvider, ILogger<EmployeeSyncBackgroundService> logger)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<EmployeeSyncBackgroundService> _logger = logger;

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 600 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task SyncEmployeeDataAsync(string orgCode, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var hrService = scope.ServiceProvider.GetRequiredService<HrService>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Lấy dữ liệu từ HRIS
            var hrEmployees = await hrService.GetEmployeesByOrgCode(orgCode, cancellationToken);
            if (hrEmployees is null || hrEmployees.Count == 0)
            {
                _logger.LogWarning("Không tìm thấy dữ liệu nhân sự nào từ HRIS.");
                return;
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 2. Xóa toàn bộ dữ liệu bảng Temp
                    context.EmployeeTemps.RemoveRange(context.EmployeeTemps);
                    await context.SaveChangesAsync(cancellationToken);

                    // 3. Insert dữ liệu mới vào bảng Temp
                    var tempEntities = hrEmployees.Select(e => new EmployeeTemp
                    {
                        OrgCode = e.OrgCode,
                        EmployeeNo = e.Code,
                        Name = e.EmployeeName,
                        Email = e.Email,
                        DeptCode = e.DeptCode,
                        DeptName = e.DeptName,
                        DeptLevel = e.DeptLevel,
                        TitleCode = e.TitleCode,
                        TitleName = e.TitleName,
                        PositionCode = e.PositionCode,
                        PositionName = e.PositionName,
                        IsResigned = e.IsResigned ?? "N"
                    }).ToList();

                    await context.EmployeeTemps.AddRangeAsync(tempEntities, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);

                    // 4. Đồng bộ Department
                    var uniqueDepartments = hrEmployees
                        .Where(e => e.IsResigned == "N" && !string.IsNullOrEmpty(e.DeptCode))
                        .GroupBy(e => e.DeptCode)
                        .Select(g => new { DeptCode = g.Key, g.First().DeptName })
                        .ToList();

                    foreach (var dept in uniqueDepartments)
                    {
                        var existingDept = await context.Departments
                            .FirstOrDefaultAsync(d => d.Code == dept.DeptCode, cancellationToken);

                        if (existingDept is null)
                        {
                            var newDept = new Department
                            {
                                Code = dept.DeptCode!,
                                Name = dept.DeptName ?? string.Empty,
                                IsActive = true
                            };
                            context.Add(newDept);
                        }
                        else if (existingDept.Name != dept.DeptName)
                        {
                            existingDept.Name = dept.DeptName ?? string.Empty;
                        }
                    }
                    await context.SaveChangesAsync(cancellationToken);

                    // Tạo từ điển (Dictionary) tra cứu nhanh DepartmentId dựa vào DeptCode vừa đồng bộ xong
                    var deptDictionary = await context.Departments
                        .ToDictionaryAsync(d => d.Code, d => d.Id, cancellationToken);

                    // 5.1 Cập nhật nhân viên chưa nghỉ việc (IsResigned = 'N')
                    var activeEmployees = hrEmployees.Where(e => e.IsResigned == "N").ToList();
                    foreach (var hrEmp in activeEmployees)
                    {
                        // Lấy ra DepartmentId tương ứng với DeptCode của nhân viên
                        int? mappedDepartmentId = null;
                        if (!string.IsNullOrEmpty(hrEmp.DeptCode) && deptDictionary.TryGetValue(hrEmp.DeptCode, out var deptId))
                        {
                            mappedDepartmentId = deptId;
                        }

                        var existingEmp = await context.Employees
                            .FirstOrDefaultAsync(e => e.EmployeeNo == hrEmp.Code, cancellationToken);

                        if (existingEmp is null)
                        {
                            context.Employees.Add(new Employee
                            {
                                EmployeeNo = hrEmp.Code,
                                Name = hrEmp.EmployeeName,
                                Email = hrEmp.Email,
                                Title = hrEmp.TitleName,
                                DepartmentId = mappedDepartmentId,
                                IsActive = true,
                                Source = "HRIS"
                            });
                        }
                        else
                        {
                            // Đã tồn tại -> Kiểm tra xem có field nào thay đổi hay không
                            bool hasChanged = false;

                            if (existingEmp.Name != hrEmp.EmployeeName)
                            {
                                existingEmp.Name = hrEmp.EmployeeName;
                                hasChanged = true;
                            }

                            if (existingEmp.Email != hrEmp.Email)
                            {
                                existingEmp.Email = hrEmp.Email;
                                hasChanged = true;
                            }

                            if (existingEmp.DepartmentId != mappedDepartmentId)
                            {
                                existingEmp.DepartmentId = mappedDepartmentId;
                                hasChanged = true;
                            }

                            if (existingEmp.Title != hrEmp.TitleName)
                            {
                                existingEmp.Title = hrEmp.TitleName;
                                hasChanged = true;
                            }

                            if (!existingEmp.IsActive)
                            {
                                existingEmp.IsActive = true;
                                hasChanged = true;
                            }

                            if (!hasChanged)
                            {
                                continue;
                            }
                        }
                    }

                    // 5.2 Cập nhật nhân viên đã nghỉ việc (IsResigned = 'Y')
                    var resignedEmployees = hrEmployees.Where(e => e.IsResigned == "Y").ToList();
                    foreach (var hrEmp in resignedEmployees)
                    {
                        var existingEmp = await context.Employees
                            .FirstOrDefaultAsync(e => e.EmployeeNo == hrEmp.Code, cancellationToken);

                        if (existingEmp != null)
                        {
                            existingEmp.IsActive = false;
                        }
                    }
                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }

            });
        }
    }
}
