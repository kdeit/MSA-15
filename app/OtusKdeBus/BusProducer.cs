using System.Text.Json;
using OtusKdeBus.Model;
using RabbitMQ.Client;

namespace OtusKdeBus;

public class BusProducer : IBusProducer
{
    private IModel _channel;

    public BusProducer()
    {
        var factory = new ConnectionFactory()
            { HostName = "localhost", VirtualHost = "otus", Port = 5672, UserName = "admin", Password = "sEcret" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public void SendMessage<T>(T message) where T : BaseEvent
    {
        Send(message.GetEventType(), message);
    }

    private void Send<T>(MessageType type, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "user_exchange", routingKey: $"Routing_key_{type}", basicProperties: null,
            body: body);
    }
}