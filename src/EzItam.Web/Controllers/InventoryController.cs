using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace EzItam.Web.Controllers
{
    public class InventoryController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : BaseController(api, loc)
    {
        public async Task<IActionResult> Index()
        {
            var inventories = await Api.ListInventoryAsync();
            return View(inventories);
        }

        [Authorize(Policy = "CanEdit")]
        public IActionResult Create()
        {
            {
                return View("Form", new InventoryDto());
            }
        }


        [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryDto model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            try
            {
                bool res = await Api.CreateInventoryAsync(model);
                Success(L["inv.create.successfully"]);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex)
            {
                AddApiError(ex);
                return View("Form", model);
            }
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await Api.GetInventoryByIdAsync(id);
            return View("Form", model);
        }

        [HttpPost, Authorize(Policy = "CanEdit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryDto model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View("Form", model);
            try
            {
                await Api.UpdateInventoryAsync(id, model);
                Success(L["inv.update.successfully"]);
                return RedirectToAction(nameof(Index));
            }
            catch (ApiException ex) {
                AddApiError(ex);
                return View("Form", model); 
            }
        }

        [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await Api.DeleteInventoryAsync(id);
                Success(L["inv.delete.successfully"]);
            }
            catch (ApiException ex) 
            { 
                Error(L[ex.Message]); 
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> Genbook(int id)
        {
            try
            {
                var res = await Api.GenInventoryBookAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                Error(L[ex.Message]);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var inventories = await Api.GetInventoryDetailByInventoryIdAsync(id);
            return View(inventories);
        }

        public async Task<IActionResult> Scan(int id)
        {
            var inventory = await Api.GetInventoryByIdAsync(id);
            if(inventory is null)
            {
                return NotFound();
            }
            return View(inventory);
        }
    }
}
