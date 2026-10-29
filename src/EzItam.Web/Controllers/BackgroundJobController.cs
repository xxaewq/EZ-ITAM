using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EzItam.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class BackgroundJobController(IConfiguration configuration) : Controller
    {
        private readonly IConfiguration _configuration = configuration;
        public IActionResult Index()
        {
            var hangFireUrl = String.Concat(_configuration.GetValue<string>("Api:BaseUrl"),"Hangfire") ;
            ViewBag.HangfireUrl = hangFireUrl;
            return View();
        }
    }
}
