using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtusKdeDAL;

namespace OtusKdeBilling.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController : ControllerBase
{
    private readonly ILogger<WalletController> _logger;
    private readonly BillingContext _cnt;

    public WalletController(ILogger<WalletController> logger, BillingContext cnt)
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
}