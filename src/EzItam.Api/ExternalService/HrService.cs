using Microsoft.Data.SqlClient;
using System.Threading.RateLimiting;

namespace EzItam.Api.ExternalService
{
    public class HrService(IConfiguration configuration, AppDbContext db)
    {
        private readonly string _connectionString = configuration.GetConnectionString("HRIS_Db") ?? throw new Exception("HRIS Database connection is null.");
        private readonly AppDbContext _db = db;

        public async Task<List<HrEmployeeDto>> GetEmployeesByOrgCode(string orgCode, CancellationToken cancellationToken = default)
        {
            var employees = new List<HrEmployeeDto>();

            var sql = @"
            WITH TreeDept AS (
                SELECT ID, ParentID, Code, OrgStructureName, OrgStructureLevel
                FROM D03_OrgStructure
                WHERE Code = @OrgCode
                
                UNION ALL
                
                SELECT d.ID, d.ParentID, d.Code, d.OrgStructureName, d.OrgStructureLevel
                FROM D03_OrgStructure d
                INNER JOIN TreeDept t ON d.ParentID = t.ID
            )
            SELECT 
                e.Code,
                e.EmployeeName,
                null as Email,
                td.Code AS DeptCode,
                td.OrgStructureName AS DeptName,
                td.OrgStructureLevel AS DeptLevel, 
                tt.Code as TitleCode,
                tt.JobTitleName as TitleName, 
                p.Code as PositionCode, 
                p.PositionName as PositionName,
                CASE 
                    WHEN e.ResignNo is not null THEN 'Y'
                    ELSE 'N'
                END AS IsResigned
            FROM D04_Employee e 
            INNER JOIN TreeDept td ON e.OrgStructureID = td.ID 
            LEFT JOIN C04_JobTitle tt ON e.JobTitleID = tt.ID 
            LEFT JOIN C04_Position p ON e.PositionID = p.ID 
            WHERE e.IsDelete != 1 or e.IsDelete is null
            ORDER BY td.OrgStructureLevel, e.Code ASC";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@OrgCode", orgCode);

            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                employees.Add(new HrEmployeeDto
                {
                    Code = reader["Code"]?.ToString() ?? string.Empty,
                    EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? string.Empty,
                    DeptCode = reader["DeptCode"]?.ToString() ?? string.Empty,
                    DeptName = reader["DeptName"]?.ToString() ?? string.Empty,
                    DeptLevel = reader["DeptLevel"]?.ToString() ?? string.Empty,
                    TitleCode = reader["TitleCode"]?.ToString() ?? string.Empty,
                    TitleName = reader["TitleName"]?.ToString() ?? string.Empty,
                    PositionCode = reader["PositionCode"]?.ToString() ?? string.Empty,
                    PositionName = reader["PositionName"]?.ToString() ?? string.Empty,
                    IsResigned = reader["IsResigned"]?.ToString() ?? string.Empty,
                });
            }
            await reader.CloseAsync();
            await connection.CloseAsync();
            return employees;
        }


        public async Task<List<HrEmployeeDto>> GetEmployeesByOrgCodeMock(string orgCode, CancellationToken cancellationToken = default)
        {
            List<HrEmployeeDto> res = [
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00001",
                    EmployeeName = "NGUYEN VAN A",
                    Email = "anv00001@test.com",
                    DeptCode = "ABC",
                    DeptName = "ABC",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00002",
                    EmployeeName = "NGUYEN VAN B",
                    Email = "bnv00002@test.com",
                    DeptCode = "AB1",
                    DeptName = "AB1",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00003",
                    EmployeeName = "NGUYEN VAN C",
                    Email = "cnv00003@test.com",
                    DeptCode = "AB2",
                    DeptName = "AB2",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "Y"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00004",
                    EmployeeName = "NGUYEN VAN D",
                    Email = "dnv00004@test.com",
                    DeptCode = "AB2",
                    DeptName = "AB2",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00005",
                    EmployeeName = "NGUYEN VAN E",
                    Email = "env00005@test.com",
                    DeptCode = "AB5",
                    DeptName = "AB5",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00006",
                    EmployeeName = "NGUYEN VAN F",
                    Email = "fnv00006@test.com",
                    DeptCode = "ABC",
                    DeptName = "ABC",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00007",
                    EmployeeName = "NGUYEN VAN G",
                    Email = "gnv00007@test.com",
                    DeptCode = "ABC",
                    DeptName = "ABC",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00008",
                    EmployeeName = "NGUYEN VAN H",
                    Email = "hnv00008@test.com",
                    DeptCode = "AB8",
                    DeptName = "AB8",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "Y"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00009",
                    EmployeeName = "NGUYEN VAN I",
                    Email = "inv00009@test.com",
                    DeptCode = "AB9",
                    DeptName = "AB9",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "N"
                },
                new HrEmployeeDto()
                {
                    OrgCode = "HYF",
                    Code = "00010",
                    EmployeeName = "NGUYEN VAN J",
                    Email = "jnv00010@test.com",
                    DeptCode = "AB10",
                    DeptName = "AB10",
                    DeptLevel = "lv3",
                    TitleCode = "X",
                    TitleName = "X",
                    PositionCode = "X",
                    PositionName = "X",
                    IsResigned = "Y"
                }
            ]
            ;


            return res;
        }

    }
}
