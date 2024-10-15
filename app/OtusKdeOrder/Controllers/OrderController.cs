using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OtusKdeDAL;

namespace OtusKdeBilling.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly OrderContext _cnt;

    public OrderController(ILogger<OrderController> logger, OrderContext cnt)
    {
        _logger = logger;
        _cnt = cnt;
    }

    [HttpPost]
    public async Task<ActionResult<decimal>> GetAmountForUser([FromHeader(Name = "X-user-id")] [Required] int UserId,
        OrderCreateRequest model)
    {
        _logger.LogInformation("Create order for user {0}", UserId);
        var nv = new Order()
        {
            Status = OrderStatus.CREATED,
            UserId = UserId,
            Total = model.Total,
            CreatedAt = DateTime.UtcNow
        };
        await _cnt.Orders.AddAsync(nv);
        await _cnt.SaveChangesAsync();

        return Ok();
    }
}