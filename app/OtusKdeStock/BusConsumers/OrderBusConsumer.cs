using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtuKdeStock.BusConsumers;

public class OrderBusConsumer
{
    private IBusConsumer _consumer;
    private IBusProducer _producer;

    public OrderBusConsumer(IBusConsumer busConsumer, IBusProducer producer)
    {
        _consumer = busConsumer;
        _producer = producer;
    }

    public void Init()
    {
        Action<OrderCreatedEvent> action = async (x) =>
        {
            var random = new Random();
            Thread.Sleep(random.Next(300, 1000));
            var b = random.Next(2);
            Console.WriteLine($"Check stock for order:: {x.OrderId} with result ${b}");
            if (b == 1)
            {
                _producer.SendMessage(new StockOrderConfirmedEvent() { OrderId = x.OrderId });
            }
            else
            {
                _producer.SendMessage(new StockOrderRejectedEvent() { OrderId = x.OrderId });
            }
        };
        _consumer.OnOrderCreated("raisin", action);
    }
}