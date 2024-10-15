using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

namespace OtusKdeBilling.Controllers;

[ApiController]
[Route("[controller]")]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;
    private readonly BillingContext _cnt;

    public BillingController(ILogger<BillingController> logger, BillingContext cnt)
    {
        _logger = logger;
        _cnt = cnt;
    }

    [HttpGet]
    public async Task<ActionResult<decimal>> GetAmountForUser([FromHeader(Name = "X-user-id")] [Required] int UserId)
    {
        _logger.LogInformation("Get amount value for user {0}", UserId);
        var res = await _cnt.Wallets.FirstOrDefaultAsync(_ => _.UserId == UserId);
        if (res is null) return NotFound();

        return Ok(res.Amount);
    }
    
    [HttpGet("increase")]
    public async Task<ActionResult> Increase([FromHeader(Name = "X-user-id")] [Required] int UserId, [FromQuery] decimal value)
    {
        _logger.LogInformation("Increse amount value for user {0} with {1}", UserId, value);
        var res = await _cnt.Wallets.FirstOrDefaultAsync(_ => _.UserId == UserId);
        if (res is null) return NotFound();
        res.Amount += value;
        await _cnt.SaveChangesAsync();

        return Ok();
    }
}