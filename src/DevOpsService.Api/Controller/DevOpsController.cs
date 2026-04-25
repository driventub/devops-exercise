using DevOpsService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsService.Api.Controllers;

[ApiController]
public class DevOpsController : ControllerBase
{
    [HttpPost("/DevOps")]
    public IActionResult Post([FromBody] Request request)
    {
        var response = new DevOpsResponse
        {
            Message = $"Hello {request.To} your message will be sent"
        };

        return Ok(response);
    }
}