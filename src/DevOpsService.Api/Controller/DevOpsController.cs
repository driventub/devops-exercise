using DevOpsService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsService.Api.Controller
{
    [ApiController]
    public sealed class DevOpsController : ControllerBase
    {
        [HttpPost("/DevOps")]
        public IActionResult Post([FromBody] Request request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            DevOpsResponse response = new()
            {
                Message = $"Hello {request.To} your message will be sent"
            };

            return Ok(response);
        }
    }
}

