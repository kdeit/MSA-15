using OtusKdeBus.Model;

namespace OtusKdeBus;

public interface IBusProducer
{
    public void SendMessage<T>(T message) where T: BaseEvent;
}