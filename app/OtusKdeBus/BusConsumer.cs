using System.Text;
using System.Text.Json;
using OtusKdeBus.Model.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OtusKdeBus;

public class BusConsumer : IBusConsumer
{
    private IModel _channel;

    public BusConsumer()
    {
        var factory = new ConnectionFactory()
            { HostName = "localhost", VirtualHost = "otus", Port = 5672, UserName = "admin", Password = "sEcret" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    public void OnClientCreated(Action<UserCreatedEvent> fn)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            //Console.WriteLine($"message {message}");
            var res = JsonSerializer.Deserialize<UserCreatedEvent>(message); 
            Console.WriteLine($"res {res.UserId}");
            fn(res);
        };

        _channel.BasicConsume("ClientQueue2",
            autoAck: true,
            consumer: consumer);
    }
}