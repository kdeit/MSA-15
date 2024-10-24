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
        var debit = await _cnt.Wallets.Where(_ => _.UserId == UserId).Select(_ => _.Value).ToListAsync();
        var credit = await _cnt.Payments.Where(_ => _.UserId == UserId && _.Status == PaymentsStatus.ACCEPTED)
            .Select(_ => _.Value)
            .ToListAsync();
        return Ok(debit.Sum() - credit.Sum());
    }

    [HttpGet("increase")]
    public async Task<ActionResult> Increase([FromHeader(Name = "X-user-id")] [Required] int UserId,
        [FromQuery] decimal value)
    {
        _logger.LogInformation("Increse amount value for user {0} with {1}", UserId, value);
        _cnt.Wallets.AddAsync(new Wallets()
        {
            UserId = UserId, Value = value
        });
        await _cnt.SaveChangesAsync();


        return Ok();
    }
}