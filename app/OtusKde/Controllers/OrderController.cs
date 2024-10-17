using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OtusKdeDAL;

namespace OtusKdeGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : Controller
{
    private readonly HttpClient _http;
    private readonly HttpClient _http2;
    private readonly IHostEnvironment _env;

    public OrderController(HttpClient http, HttpClient http2, IHostEnvironment env)
    {
        _env = env;
        _http = http;
        _http.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:5015")
            : new Uri("http://api-client-asp.default.svc.cluster.local");
        
        _http2 = http2;
        _http2.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:5225")
            : new Uri("http://api-order-asp.default.svc.cluster.local");
        
    }
    
    [HttpGet("{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Get(int orderId)
    {
        var userId = await GetUserId();
        var res = await _http2.GetAsync($"order/{orderId}/{userId}");
        if (!res.IsSuccessStatusCode) return NotFound();
        Order content = await res.Content.ReadFromJsonAsync<Order>();

        return Ok(content);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Create(OrderCreateRequest model)
    {
        var userId = await GetUserId();
        _http2.DefaultRequestHeaders.Add("X-user-id", userId.ToString());
        
        JsonContent content = JsonContent.Create(model);
        var resp0 = await _http2.PostAsync($"order/", content);
        if (!resp0.IsSuccessStatusCode) return BadRequest();
        var res = await resp0.Content.ReadAsStringAsync();

        return Ok(res);
    }
    private async Task<int> GetUserId()
    {
        var email = HttpContext.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        var res0 = await _http.GetAsync($"user/{email}");
        User content = await res0.Content.ReadFromJsonAsync<User>();

        return content.Id;
    }
}