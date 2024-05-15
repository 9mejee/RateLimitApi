using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace RateLimitingApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController(ILogger<HomeController> _logger) : ControllerBase
{        
    [HttpGet]
    public ActionResult GetAll()
    {
        return Ok("Hit Successfully.");
    }

    [HttpGet]
    [EnableRateLimiting("Sliding")]
    public ActionResult Get()
    {
        return Ok("Hit Successfully.");
    }
}
