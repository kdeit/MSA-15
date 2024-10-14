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
        
        var email = HttpContext.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email is null) return NotFound();
        var res0 = await _http.GetAsync($"user/{email}");
        if (!res0.IsSuccessStatusCode) return NotFound();
        User content = await res0.Content.ReadFromJsonAsync<User>();
        
        
        _http2.DefaultRequestHeaders.Add("X-user-id", content.Id.ToString());
        var res1 = await _http2.GetAsync($"/wallet");
        if (!res1.IsSuccessStatusCode) return NotFound();
        string amount = await res1.Content.ReadAsStringAsync();
        
        return Ok(Convert.ToDecimal(amount));
    }
}