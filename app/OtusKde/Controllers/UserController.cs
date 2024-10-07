using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtusKdeDAL;

namespace OtusKde.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly HttpClient _http;
    public UserController(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("http://localhost:5015");
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetById()
    {
        var email = HttpContext.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email is null) return NotFound();
        var res = await _http.GetAsync($"user/{email}");
        if (!res.IsSuccessStatusCode) return NotFound();
        string content = await res.Content.ReadAsStringAsync();
         
        return Ok(content);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Create(UserCreateUpdateRequest model)
    {
        JsonContent content = JsonContent.Create(model);
        var res = await _http.PostAsync($"user/", content);
        
        return Ok(res.IsSuccessStatusCode);
    }
    
    [HttpPut]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(UserCreateUpdateRequest model)
    {
        var email = HttpContext.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (email is null || email != model.Email) return NotFound();
        JsonContent content = JsonContent.Create(model);
        var res = await _http.PutAsync($"user/", content);
        
        return Ok(res.IsSuccessStatusCode);
    }
}