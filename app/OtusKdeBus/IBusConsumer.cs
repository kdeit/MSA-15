using OtusKdeBus.Model.Client;

namespace OtusKdeBus;

public interface IBusConsumer
{
    public void OnClientUserCreated(string queue_name, Action<ClientUserCreatedEvent> fn);
    public void OnOrderCreated(string queue_name, Action<OrderCreatedEvent> fn);
    public void OnOrderConfirmed(string queue_name, Action<OrderConfirmedEvent> fn);
    public void OnOrderReverted(string queue_name, Action<OrderRevertedEvent> fn);
    
}