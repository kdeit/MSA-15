using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtusKdeDAL.BusConsumers;

public class ClientBusConsumer
{
    private IBusConsumer _consumer;
    private IBusProducer _producer;
    private BillingContext _cnt;

    public ClientBusConsumer(IBusConsumer busConsumer, BillingContext context, IBusProducer producer)
    {
        _consumer = busConsumer;
        _cnt = context;
        _producer = producer;
    }

    public void Init()
    {
        Action<ClientUserCreatedEvent> action = (x) =>
        {
            Console.WriteLine(x.UserId);
            _cnt.Wallets.AddAsync(new Wallet()
            {
                UserId = x.UserId,
                Amount = 0
            });
            _cnt.SaveChangesAsync();
        };
        _consumer.OnClientUserCreated("melon", action);
        
    }
}