using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Test : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}