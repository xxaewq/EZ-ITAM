using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzItam.Api.Controllers
{

    [ApiController]
    [Route("api/v1/inventories")]
    public class InventoryController(IInventoryService inventoryService) : ControllerBase
    {
        private readonly IInventoryService _inventoryService = inventoryService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventories()
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(inventories);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventoryDetailResponseDto>> GetInventoryById(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory is null)
            {
                return NotFound();
            }
            return Ok(inventory);
        }

        [HttpPost]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult<int>> CreateInventory(CreateInventoryDto dto)
        {
            var createdInventory = await _inventoryService.CreateInventoryAsync(dto);
            return Ok(createdInventory);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult> UpdateInventory(int id, UpdateInventoryDto dto)
        {
            var updated = await _inventoryService.UpdateInventoryAsync(id, dto);
            if (!updated)
            {
                return NotFound();
            }
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteInventory(int id)
        {
            var deleted = await _inventoryService.DeleteInventoryAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return Ok(deleted);
        }

        [HttpPut("{id:int}/close")]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult> CloseInventory(int id)
        {
            var deleted = await _inventoryService.CloseInventoryAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPost("{id:int}/genbook")]
        [Authorize(Policy = "CanEdit")]
        public async Task<ActionResult<int>> GenInventoryBook(int id)
        {
            var res = await _inventoryService.GenInventoryBookAsync(id);
            return Ok(res);
        }

    }
}
