using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusKdeDAL;

namespace OtusKdeGate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : Controller
{
    private readonly HttpClient _http;
    private readonly HttpClient _http2;
    private readonly IHostEnvironment _env;

    public WalletController(HttpClient http, HttpClient http2, IHostEnvironment env)
    {
        _env = env;
        _http = http;
        _http.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:5015")
            : new Uri("http://api-client-asp.default.svc.cluster.local");
        
        _http2 = http2;
        _http2.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:5166")
            : new Uri("http://api-billing-asp.default.svc.cluster.local");
        
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> Get()
    {
        var userId = await GetUserId();
        _http2.DefaultRequestHeaders.Add("X-user-id", userId.ToString());
        var res1 = await _http2.GetAsync($"/wallet");
        if (!res1.IsSuccessStatusCode) return NotFound();
        string amount = await res1.Content.ReadAsStringAsync();
        
        return Ok(Convert.ToDecimal(amount));
    }
    
    [HttpGet("increase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Get([FromQuery] decimal value)
    {
        var userId = await GetUserId();
        _http2.DefaultRequestHeaders.Add("X-user-id", userId.ToString());
        var res = await _http2.GetAsync($"/wallet/increase?value={value}");
            
        return Ok(res.IsSuccessStatusCode);
    }

    private async Task<int> GetUserId()
    {
        var email = HttpContext.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        var res0 = await _http.GetAsync($"user/{email}");
        User content = await res0.Content.ReadFromJsonAsync<User>();

        return content.Id;
    }
}