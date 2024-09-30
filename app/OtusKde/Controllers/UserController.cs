using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> GetById(int id)
    {
        var res = await _http.GetAsync($"user/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        string content = await res.Content.ReadAsStringAsync();
         
        return Ok(content);
    }
}