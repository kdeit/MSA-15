using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtusKdeDAL.BusConsumers;

public class OrderBusConsumer
{
    private IBusConsumer _consumer;
    private IBusProducer _producer;
    private BillingContext _cnt;

    public OrderBusConsumer(IBusConsumer busConsumer, BillingContext context, IBusProducer producer)
    {
        _consumer = busConsumer;
        _cnt = context;
        _producer = producer;
    }

    public void Init()
    {
        Action<OrderCreatedEvent> action = async (x) =>
        {
            var value = Convert.ToDecimal(x.Total);
            var wallet = _cnt.Wallets
                .FirstOrDefault(_ => _.UserId == x.UserId);
            await _cnt.Entry(wallet).ReloadAsync();
            if (wallet is null || wallet.Amount < value)
            {
                Console.WriteLine($"Rejected. Amount:: {wallet.Amount} Total:: {value}" );
                _producer.SendMessage(new BillingOrderRejectedEvent() { OrderId = x.OrderId });
                return;
            }
            Console.WriteLine("Confirmed");

            wallet.Amount -= value;
            await _cnt.SaveChangesAsync();
            _producer.SendMessage(new BillingOrderConfirmedEvent() { OrderId = x.OrderId });
        };
        _consumer.OnOrderCreated("watermelon", action);
    }
}