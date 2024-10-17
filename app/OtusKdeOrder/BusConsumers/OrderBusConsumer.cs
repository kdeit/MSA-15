using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeBus.Model.Client;
using OtusKdeDAL;

namespace OtusKdeOrder.BusConsumers;

public class OrderBusConsumer
{
    private IBusConsumer _consumer;
    private IBusProducer _producer;
    private OrderContext _cnt;

    public OrderBusConsumer(IBusConsumer busConsumer, OrderContext context, IBusProducer producer)
    {
        _consumer = busConsumer;
        _cnt = context;
        _producer = producer;
    }

    public void Init()
    {
        Action<OrderConfirmedEvent> action = async (x) =>
        {
            Console.WriteLine(x.OrderId);
            var order = await _cnt.Orders.FirstOrDefaultAsync(_ => _.Id == x.OrderId);
            if (order is null) return;
            order.Status = OrderStatus.CONFIRMED;
            _cnt.SaveChangesAsync();
        };
        _consumer.OnOrderConfirmed("banan", action);
        
        Action<OrderRevertedEvent> action2 = async (x) =>
        {
            Console.WriteLine(x.OrderId);
            var order = await _cnt.Orders.FirstOrDefaultAsync(_ => _.Id == x.OrderId);
            if (order is null) return;
            order.Status = OrderStatus.REJECTED;
            _cnt.SaveChangesAsync();
        };
        _consumer.OnOrderReverted("banana", action2);

        
    }
}