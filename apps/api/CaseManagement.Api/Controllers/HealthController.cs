using Microsoft.AspNetCore.Mvc;

namespace CaseManagement.Api.Controllers;

[ApiController]
[Route("")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetRoot()
    {
        return Ok(new
        {
            name = "Case Management Api",
            status = "ok"
        });
    }
}