using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OtusKde.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : Controller
{
    private readonly IConfiguration _config;
    public PingController(IConfiguration config)
    {
        _config = config;
    }
    [HttpGet]
    [Authorize]
    public ActionResult<string> Pong()
    {
        
        return Ok("Hello");
    }
}