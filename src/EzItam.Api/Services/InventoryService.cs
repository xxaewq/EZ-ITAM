using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace EzItam.Api.Services
{
    public interface IInventoryService
    {
        // --- Quản lý đợt kiểm kê (Inventory Management) ---
        Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
        Task<InventoryDto?> GetInventoryByIdAsync(int inventoryId);
        Task<bool> CreateInventoryAsync(CreateInventoryDto dto);
        Task<bool> UpdateInventoryAsync(int id, UpdateInventoryDto dto);
        Task<bool> CloseInventoryAsync(int id); // Đóng/Hoàn thành đợt kiểm kê
        Task<bool> GenInventoryBookAsync(int id);
        Task<bool> DeleteInventoryAsync(int id);
        Task<bool> IsAnyNotCloseInvetoryAsync();
        Task<InventoryDetailResponseDto?> GetInventoryDetailByInventoryIdAsync(int id);
        Task<InventoryDetailDto?> GetDetailByTag(int inventoryId, string tag);
        Task<bool> PostScanResult(ScanResultRequest req);
        Task<bool> PostDiff(int inventoryId);
    }



    public class InventoryService(AppDbContext context, ICurrentUserService currentUser) : IInventoryService
    {
        private readonly AppDbContext _context = context;
        private readonly ICurrentUserService _currentUser = currentUser;

        public async Task<bool> IsAnyNotCloseInvetoryAsync()
        {
            return await _context.Inventories.AnyAsync(d => d.InventoryStatus != InventoryStatus.Closed);
        }

        public async Task<bool> CloseInventoryAsync(int id)
        {
            var inventory = _context.Inventories.Find(id);
            if (inventory is null || inventory.InventoryStatus != InventoryStatus.InProgress)
            {
                return false;
            }

            inventory.InventoryStatus = InventoryStatus.Closed;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateInventoryAsync(CreateInventoryDto dto)
        {
            var isAnyNotClose = await IsAnyNotCloseInvetoryAsync();
            if (isAnyNotClose) throw new DomainException("There are other inventory that have not yet been closed.");
            var inventory = new Inventory
            {
                Name = dto.Name,
                DateFrom = dto.DateFrom,
                DateTo = dto.DateTo,
                Notes = dto.Notes,
                InventoryStatus = InventoryStatus.Open
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GenInventoryBookAsync(int id)
        {

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory is null || inventory.InventoryStatus != InventoryStatus.Open)
            {
                throw new DomainException("Inventory not found or not open.");
            }

            // Kiểm tra thời gian (Tùy chọn: Nếu muốn cho phép tạo trước ngày bắt đầu thì bỏ qua điều kiện DateFrom)
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (inventory.DateTo < today)
            {
                throw new DomainException("The inventory is not within the valid date range.");
            }

            // 2. Kiểm tra xem đợt kiểm kê này đã được đồng bộ danh sách thiết bị trước đó chưa
            if (inventory.InventoryStatus == InventoryStatus.InProgress)
            {
                throw new DomainException("The inventory has already been synchronized.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var allAssetsExceptDisposedAndLost = await _context.Assets.Where(d => d.Status != AssetStatus.Disposed && d.Status != AssetStatus.Lost).ToListAsync();

                    if (allAssetsExceptDisposedAndLost.Count == 0)
                    {
                        throw new DomainException("No assets found in the system to inventory.");
                    }

                    var inventoryDetails = allAssetsExceptDisposedAndLost.Select(asset => new InventoryDetail
                    {
                        InventoryId = id,
                        IdAsset = asset.Id,
                        Status = asset.Status,
                        IdLocation = asset.LocationId,
                        IdEmployee = asset.CurrentEmployeeId,

                        IdLocationIV = asset.LocationId,
                        AssetStatusIV = asset.Status,
                        IdEmployeeIV = asset.CurrentEmployeeId,

                        UpdatedBy = _currentUser.UserName,
                        UpdatedTime = DateTime.Now
                    }).ToList();

                    _context.InventoryDetails.AddRange(inventoryDetails);

                    inventory.InventoryStatus = InventoryStatus.InProgress;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            });
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var model = await _context.Inventories.FindAsync(id) ?? throw new DomainException("Not found.");

            if (model.InventoryStatus == InventoryStatus.Closed)
            {
                throw new DomainException("Can't delete.");
            }

            _context.Inventories.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
        {
            var inventories = _context.Inventories
                .AsNoTracking()
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    DateFrom = i.DateFrom,
                    DateTo = i.DateTo,
                    Notes = i.Notes,
                    InventoryStatus = i.InventoryStatus,
                    ScannedAssetsCount = i.InventoryDetails.Count(d => d.ScanCount > 0),
                    TotalAssetsCount = i.InventoryDetails.Count()
                });
            return await inventories.ToListAsync();
        }

        public async Task<InventoryDto?> GetInventoryByIdAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory is null)
            {
                return null;
            }

            return new InventoryDto
            {
                Id = inventory.Id,
                Name = inventory.Name,
                DateFrom = inventory.DateFrom,
                DateTo = inventory.DateTo,
                Notes = inventory.Notes,
                InventoryStatus = inventory.InventoryStatus
            };
        }

        public async Task<bool> UpdateInventoryAsync(int id, UpdateInventoryDto dto)
        {
            var inventory = await _context.Inventories.FindAsync(id);

            if (inventory is null)
            {
                return false;
            }

            if (inventory.InventoryStatus == InventoryStatus.Closed)
            {
                throw new DomainException("Unable to update the closed inventory.");
            }

            inventory.Name = dto.Name;
            inventory.DateFrom = dto.DateFrom;
            inventory.DateTo = dto.DateTo;
            inventory.Notes = dto.Notes;
            inventory.InventoryStatus = dto.InventoryStatus;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InventoryDetailResponseDto?> GetInventoryDetailByInventoryIdAsync(int id)
        {
            // 1. Lấy thông tin chung của đợt kiểm kê (Inventory)
            var inventory = await _context.Inventories
                .AsNoTracking()
                .Where(i => i.Id == id)
                .Select(i => new InventoryDetailResponseDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    DateFrom = i.DateFrom,
                    DateTo = i.DateTo,
                    Notes = i.Notes,
                    InventoryStatus = i.InventoryStatus
                })
                .FirstOrDefaultAsync();

            if (inventory is null)
            {
                return new();
            }

            // 2. Lấy danh sách chi tiết thiết bị kiểm kê và kết hợp các bảng liên quan
            inventory.Details = await _context.InventoryDetails
                .AsNoTracking()
                .Where(d => d.InventoryId == id)
                .Select(d => new InventoryDetailDto
                {
                    Id = d.Id,
                    IdAsset = d.IdAsset,
                    // Lấy AssetTag và AssetName từ bảng Asset thông qua quan hệ IdAsset
                    AssetTag = d.Asset != null ? d.Asset.AssetTag : null,       // Thay đổi tên thuộc tính 'Tag' nếu model Asset của bạn đặt khác (vd: AssetTag)
                    AssetName = d.Asset != null ? d.Asset.Name : null,
                    // Lấy CategoryName thông qua quan hệ từ Asset sang Category
                    CategoryName = d.Asset != null && d.Asset.Category != null ? d.Asset.Category.Name : null,

                    IdLocation = d.IdLocation ?? 0,
                    LocationName = d.Location != null ? d.Location.Name : null,

                    IdEmployee = d.IdEmployee ?? 0,
                    EmployeeName = d.Employee != null ? d.Employee.Name : null,

                    Status = d.Status,

                    IdLocationIV = d.IdLocationIV ?? 0,
                    LocationIVName = d.LocationIV != null ? d.LocationIV.Name : null,

                    IdEmployeeIV = d.IdEmployeeIV ?? 0,
                    EmployeeIVName = d.EmployeeIV != null ? d.EmployeeIV.Name : null,

                    StatusIV = d.AssetStatusIV,

                    ScanCount = d.ScanCount,
                    Notes = d.Notes,
                    UpdatedBy = d.UpdatedBy,
                    UpdatedTime = d.UpdatedTime
                })
                .ToListAsync();

            return inventory;
        }

        public async Task<InventoryDetailDto?> GetDetailByTag(int inventoryId, string tag)
        {
            var invDetail = await _context.InventoryDetails
                .AsNoTracking()
                .Include(d => d.Asset)
                    .ThenInclude(a => a.Category)
                .Include(d => d.Location)
                .Include(d => d.LocationIV)
                .Include(d => d.Employee)
                    .ThenInclude(d => d.Department)
                .Include(d => d.EmployeeIV)
                    .ThenInclude(d => d.Department)
                .Where(d => d.InventoryId == inventoryId && d.Asset != null && d.Asset.AssetTag == tag)
                .FirstOrDefaultAsync();
            if (invDetail is null) return new();

            return new InventoryDetailDto
            {
                Id = invDetail.Id,
                IdAsset = invDetail.IdAsset,
                AssetTag = invDetail.Asset?.AssetTag,
                CategoryName = invDetail.Asset?.Category?.Name,
                AssetName = invDetail.Asset?.Name,

                IdLocation = invDetail.IdLocation ?? 0,
                LocationName = invDetail.Location?.Name,
                IdLocationIV = invDetail.IdLocationIV ?? 0,
                LocationIVName = invDetail.LocationIV?.Name,

                IdEmployee = invDetail.IdEmployee ?? 0,
                EmployeeName = (invDetail.IdEmployee is null || invDetail.IdEmployee == 0) ? null : (invDetail.Employee?.Department != null ? invDetail.Employee?.Department.Name + " / " : "") + invDetail.Employee?.Name + " (" + invDetail.Employee?.EmployeeNo + ")",
                IdEmployeeIV = invDetail.IdEmployeeIV ?? 0,
                EmployeeIVName = (invDetail.IdEmployeeIV is null || invDetail.IdEmployeeIV == 0) ? null : (invDetail.EmployeeIV?.Department != null ? invDetail.EmployeeIV?.Department.Name + " / " : "") + invDetail.EmployeeIV?.Name + " (" + invDetail.EmployeeIV?.EmployeeNo + ")",

                Status = invDetail.Status,
                StatusIV = invDetail.AssetStatusIV,

                ScanCount = invDetail.ScanCount,
                Notes = invDetail.Notes,
                UpdatedBy = invDetail.UpdatedBy,
                UpdatedTime = invDetail.UpdatedTime
            };
        }

        public async Task<bool> PostScanResult(ScanResultRequest req)
        {
            var invAsset = await _context.InventoryDetails.FindAsync(req.Id) ?? throw new DomainException("Not found");

            invAsset.IdLocationIV = req.IdLocationIV;
            invAsset.IdEmployeeIV = req.IdEmployeeIV;
            invAsset.AssetStatusIV = (AssetStatus)req.StatusIV;
            invAsset.Notes = req.Notes;
            invAsset.ScanCount += 1;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PostDiff(int inventoryId)
        {


            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var inventory = await _context.Inventories.Where(i => i.Id == inventoryId).FirstOrDefaultAsync() ?? throw new DomainException("Inventory not found.");
                if (inventory.InventoryStatus != InventoryStatus.InProgress)
                {
                    throw new DomainException("Unable to post this inventory.");
                }

                var assets = await _context.InventoryDetails
                                    .Where(d => d.InventoryId == inventoryId && (d.IdEmployee != d.IdEmployeeIV || d.IdLocation != d.IdLocationIV || d.Status != d.AssetStatusIV))
                                    .ToListAsync();

                var assetIds = assets.Select(d => d.IdAsset).Distinct().ToList();

                var assetsToUpdate = await _context.Assets
                    .Where(a => assetIds.Contains(a.Id))
                    .ToListAsync();


                var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var item in assets)
                    {
                        var asset = assetsToUpdate.FirstOrDefault(a => a.Id == item.IdAsset);
                        if (asset != null)
                        {
                            asset.LocationId = item.IdLocationIV;
                            asset.CurrentEmployeeId = item.IdEmployeeIV;
                            asset.Status = item.AssetStatusIV;
                        }
                    }

                    inventory.InventoryStatus = inventory.InventoryStatus + 1;
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
