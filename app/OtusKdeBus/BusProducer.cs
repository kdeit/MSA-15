using System.Text.Json;
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
        _channel.QueueDeclare(queue: "ClientQueue2", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: "ClientQueue2", exchange: "user_exchange", routingKey: "ClientQueue");
    }

    public void SendClientMessage<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "user_exchange", routingKey: "ClientQueue", basicProperties: null, body: body);
    }
}