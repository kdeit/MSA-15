using System.Text;
using System.Text.Json;
using OtusKdeBus.Model;
using OtusKdeBus.Model.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OtusKdeBus;

public class BusConsumer : IBusConsumer
{
    private IModel _channel;

    public BusConsumer()
    {
        /**
         * TODO:: get other
         */

        var isProduction = Environment.GetEnvironmentVariable("DB_HOST") is not null;
        var HostName = isProduction
            ? "rabbit-rabbitmq.default.svc.cluster.local"
            : "localhost";
        Console.WriteLine($"Try connect to {HostName}...");
        
        var factory = new ConnectionFactory()
            { HostName = HostName, VirtualHost = "otus", Port = 5672, UserName = "admin", Password = "sEcret" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
    }

    private void Consume<T>(string queue_name, MessageType type, Action<T> fn)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var res = JsonSerializer.Deserialize<T>(message);
            fn(res);
        };
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange: "user_exchange",
            routingKey: $"Routing_key_{type}");

        _channel.BasicConsume($"Queue_{type}.{queue_name}",
            autoAck: true,
            consumer: consumer);
    }

    public void OnClientUserCreated(string queue_name, Action<ClientUserCreatedEvent> fn)
    {
        this.Consume(queue_name, MessageType.USER_CREATED, fn);
    }

    public void OnOrderCreated(string queue_name, Action<OrderCreatedEvent> fn)
    {
        this.Consume(queue_name, MessageType.ORDER_CREATED, fn);
    }
    
    public void OnOrderConfirmed(string queue_name, Action<OrderConfirmedEvent> fn)
    {
        this.Consume(queue_name, MessageType.ORDER_CONFIRMED, fn);
    }
    
    public void OnOrderReverted(string queue_name, Action<OrderRevertedEvent> fn)
    {
        this.Consume(queue_name, MessageType.ORDER_REVERTED, fn);
    }
    
    public void OnBillingOrderReverted(string queue_name, Action<BillingOrderRejectedEvent> fn)
    {
        this.Consume(queue_name, MessageType.BILLING_ORDER_REJECTED, fn);
    }
}