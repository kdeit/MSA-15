using OtusKdeBus.Model.Client;

namespace OtusKdeBus;

public interface IBusConsumer
{
    public void OnClientCreated(Action<UserCreatedEvent> fn);
}