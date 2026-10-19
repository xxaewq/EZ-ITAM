using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace EzItam.Web.Controllers
{
    public class InventoriesController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : BaseController(api, loc)
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
