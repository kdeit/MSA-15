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
    public ActionResult<string> Pong()
    {
        
        var res1 = Environment.GetEnvironmentVariable("DB_NAME");
        var res2 = Environment.GetEnvironmentVariable("DB_LOGIN");

        return Ok($"{res1} | {res2}" ?? "Hello");
    }
}