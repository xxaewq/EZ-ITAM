using EzItam.Api.ExternalService;
using Microsoft.AspNetCore.Mvc;

namespace EzItam.Api.Controllers.External
{
    [ApiController]
    [Route("api/v1/external/hr")]
    public class HrController(HrService hrService) : ApiControllerBase
    {
        private readonly HrService _hrService = hrService;

        [HttpGet("employees")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList([FromQuery] string orgCode, CancellationToken cancellationToken)
        {
            try
            {
                var employees = await _hrService.GetEmployeesByOrgCode(orgCode, cancellationToken);

                return Ok(new
                {
                    success = true,
                    count = employees.Count,
                    data = employees
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống khi truy vấn dữ liệu.",
                    error = ex.Message
                });
            }
        }
    }
}
