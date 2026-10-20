using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;

namespace EzItam.Api.Services
{
    public interface IInventoryService
    {
        // --- Quản lý đợt kiểm kê (Inventory Management) ---
        Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
        Task<InventoryDetailResponseDto?> GetInventoryByIdAsync(int inventoryId);
        Task<bool> CreateInventoryAsync(CreateInventoryDto dto);
        Task<bool> UpdateInventoryAsync(int id, UpdateInventoryDto dto);
        Task<bool> CloseInventoryAsync(int id); // Đóng/Hoàn thành đợt kiểm kê
        Task<bool> GenInventoryBookAsync(int id);
        Task<bool> DeleteInventoryAsync(int id);
        Task<bool> IsAnyNotCloseInvetoryAsync();
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

            inventory.InventoryStatus = InventoryStatus.Completed;
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
            if(inventory.InventoryStatus == InventoryStatus.InProgress)
            {
                throw new DomainException("The inventory has already been synchronized.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var allAssets = await _context.Assets.ToListAsync();

                    if (allAssets.Count == 0)
                    {
                        throw new DomainException("No assets found in the system to inventory.");
                    }

                    var inventoryDetails = allAssets.Select(asset => new InventoryDetail
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
                    await _context.SaveChangesAsync();

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

            if(model.InventoryStatus != InventoryStatus.Open)
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

        public async Task<InventoryDetailResponseDto?> GetInventoryByIdAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory is null)
            {
                return null;
            }

            return new InventoryDetailResponseDto
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

            if(inventory.InventoryStatus == InventoryStatus.Closed)
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
    }
}
