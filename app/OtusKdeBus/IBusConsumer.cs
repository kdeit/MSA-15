using OtusKdeBus.Model.Client;

namespace OtusKdeBus;

public interface IBusConsumer
{
    public void OnClientUserCreated(string queue_name, Action<ClientUserCreatedEvent> fn);
    public void OnOrderCreated(string queue_name, Action<OrderCreatedEvent> fn);
    public void OnBillingOrderConfirmed(string queue_name, Action<BillingOrderConfirmedEvent> fn);
    public void OnBillingOrderRejected(string queue_name, Action<BillingOrderRejectedEvent> fn);
}