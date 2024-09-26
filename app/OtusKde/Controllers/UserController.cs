using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;
using OtusKde.Models.Input;

namespace OtusKde.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly MasterContext _cnt;

    public UserController(MasterContext context)
    {
        _cnt = context;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> GetById(int id)
    {
        var user = await _cnt.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] UserCreateUpdateRequest model)
    {
        var existed = await _cnt.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existed is not null)
        {
            return BadRequest("User with email exist");
        }

        var user = new User()
        {
            Name = model.Name,
            Email = model.Email
        };
        var res = await _cnt.Users.AddAsync(user);
        await _cnt.SaveChangesAsync();

        return Ok(res.Entity.Id);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Update([FromRoute] int id, [FromBody] UserCreateUpdateRequest model)
    {
        var existed = await _cnt.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (existed is  null)
        {
            return NotFound("User not found");
        }

        var emailIsUsed = await _cnt.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != id);
        if (emailIsUsed is  not null)
        {
            return NotFound("Email is not unique");
        }

        existed.Email = model.Email;
        existed.Name = model.Name;
        await _cnt.SaveChangesAsync();

        return Ok(JsonSerializer.Serialize(existed));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Delete(int id)
    {
        var user = await _cnt.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        _cnt.Remove(user);
        await _cnt.SaveChangesAsync();
        return Ok(true);
    }
}