using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SQLitePCL;
using System.Net;

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
        public async Task<ActionResult<InventoryDto>> GetInventoryById(int id)
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

        [Authorize(Policy = "CanEdit")]
        [HttpGet("{id:int}/details")]
        public async Task<ActionResult<InventoryDetailResponseDto?>> GetInventoryDetailByInventoryId(int id)
        {
            var res = await _inventoryService.GetInventoryDetailByInventoryIdAsync(id);
            return Ok(res);
        }

        [Authorize(Policy = "CanEdit")]
        [HttpGet("{id:int}/asset")]
        public async Task<ActionResult<InventoryDetailDto?>> GetInventoryDetailByInventoryId([FromQuery] string tag, int id)
        {
            var res = await _inventoryService.GetDetailByTag(id, tag);
            return Ok(res);
        }

        [Authorize(Policy = "CanEdit")]
        [HttpPost("scan")]
        public async Task<ActionResult<bool>> ScanResult([FromBody] ScanResultRequest req)
        {
            try
            {
                var res = await _inventoryService.PostScanResult(req);
                return Ok(res);
            }
            catch (Exception)
            {
                return Ok(false);
            }
            
        }

        [Authorize(Policy = "CanEdit")]
        [HttpPost("postdiff/{id:int}")]
        public async Task<ActionResult<bool>> PostDiff(int id)
        {
            var res = await _inventoryService.PostDiff(id);
            return Ok(res);
        }
    }
}
