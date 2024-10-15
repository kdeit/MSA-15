namespace OtusKdeBus;

public interface IBusProducer
{
    public void SendClientMessage<T>(T message);
}