using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtusKdeDAL.BusConsumers;

public class BillingBusConsumer
{
    private IBusConsumer _consumer;
    private IBusProducer _producer;
    private OrderContext _cnt;

    public BillingBusConsumer(IBusConsumer busConsumer, OrderContext context, IBusProducer producer)
    {
        _consumer = busConsumer;
        _cnt = context;
        _producer = producer;
    }

    public void Init()
    {
        Action<BillingOrderConfirmedEvent> action = async (x) =>
        {
            Console.WriteLine(x.OrderId);
            var order = await _cnt.Orders.FirstOrDefaultAsync(_ => _.Id == x.OrderId);
            if (order is null) return;
            order.Status = OrderStatus.CONFIRMED;
            _cnt.SaveChangesAsync();
        };
        _consumer.OnBillingOrderConfirmed("banan", action);

        Action<BillingOrderRejectedEvent> action2 = async (x) =>
        {
            Console.WriteLine(x.OrderId);
            var order = await _cnt.Orders.FirstOrDefaultAsync(_ => _.Id == x.OrderId);
            if (order is null) return;
            order.Status = OrderStatus.REJECTED;
            _cnt.SaveChangesAsync();
        };
        _consumer.OnBillingOrderRejected("orange", action2);
    }
}