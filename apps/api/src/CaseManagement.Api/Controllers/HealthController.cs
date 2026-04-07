using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/api/health")]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "CaseManagement.Api"
        });
    }
}