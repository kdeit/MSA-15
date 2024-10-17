using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeBus.Model.Client;
using OtusKdeDAL;

namespace OtusKdeBilling.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly OrderContext _cnt;
    private readonly IBusProducer _busProducer;

    public OrderController(ILogger<OrderController> logger, OrderContext cnt, IBusProducer busProducer)
    {
        _logger = logger;
        _cnt = cnt;
        _busProducer = busProducer;
    }

    [HttpGet("{orderId}/{userId}")]
    public async Task<ActionResult<Order>> GetByIdForUser(int orderId, int userId)
    {
        var res = await _cnt.Orders.FirstOrDefaultAsync(_ => _.Id == orderId && _.UserId == userId);
        
        return res is not null ? Ok(res) : NotFound();
    }
    
    [HttpGet("{orderId}")]
    public async Task<ActionResult<Order>> GetById(int orderId)
    {
        var res = await _cnt.Orders.FirstOrDefaultAsync(_ => _.Id == orderId);
        
        return res is not null ? Ok(res) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromHeader(Name = "X-user-id")] [Required] int UserId,
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

        var @event = new OrderCreatedEvent()
        {
            UserId = UserId,
            OrderId = nv.Id,
            Total = nv.Total
        };
        _busProducer.SendMessage(@event);

        return Ok(nv.Id);
    }
}