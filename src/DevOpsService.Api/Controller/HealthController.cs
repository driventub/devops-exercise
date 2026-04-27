using Microsoft.AspNetCore.Mvc;

namespace DevOpsService.Api.Controller
{
    [ApiController]
    internal sealed class HealthController : ControllerBase
    {
        [HttpGet("/health")]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy" });
        }
    }
}