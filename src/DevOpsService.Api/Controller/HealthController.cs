using Microsoft.AspNetCore.Mvc;

namespace DevOpsService.Api.Controller
{
    [ApiController]
    public sealed class HealthController : ControllerBase
    {
        [HttpGet("/health")]
        public IActionResult Get() => Ok(new { status = "healthy" });
    }
}