using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtusKdeDAL.BusConsumers;

public class ClientBusConsumer
{
    private IBusConsumer _consumer;
    private BillingContext _cnt;

    public ClientBusConsumer(IBusConsumer busConsumer, BillingContext context)
    {
        _consumer = busConsumer;
        _cnt = context;
    }

    public void Init()
    {
        Action<UserCreatedEvent> action = (x) =>
        {
            Console.WriteLine(x.UserId);
            _cnt.Wallets.AddAsync(new Wallet()
            {
                UserId = x.UserId,
                Amount = 0
            });
            _cnt.SaveChangesAsync();
        };
        _consumer.OnClientCreated(action);
    }
}