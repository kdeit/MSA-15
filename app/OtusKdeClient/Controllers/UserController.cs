using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

namespace OtusKdeClient.Controllers;

[ApiController]
[Route("[controller]")]
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
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _cnt.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

}