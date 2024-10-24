using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using OtusKdeBus.Model;
using OtusKdeBus.Model.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OtusKdeTransaction;

enum SagaStatus
{
    PENDING,
    CONFIRMED,
    REVERTED
}

class Saga
{
    public bool IsMustBeReverted { get; set; }
    public SagaStatus Billing { get; set; }
    public SagaStatus Stock { get; set; }
    public SagaStatus Delivery { get; set; }
}

public class OrderTransactionSaga
{
    private IModel _channel;
    //private Dictionary<int, Saga> _items = new Dictionary<int, Saga>();
    private IDistributedCache _cache;


    public OrderTransactionSaga(IDistributedCache cache)
    {
        _cache = cache;
        Console.WriteLine("Start order transaction");
    }

    public void Handle()
    {
        var isProduction = Environment.GetEnvironmentVariable("DB_HOST") is not null;

        var HostName = isProduction
            ? "rabbit-rabbitmq.default.svc.cluster.local"
            : "localhost";
        Console.WriteLine($"Try connect to {HostName}...");

        var factory = new ConnectionFactory()
            { HostName = HostName, VirtualHost = "otus", Port = 5672, UserName = "admin", Password = "sEcret" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        string queue_name = "OrderTransactionSaga";
        string exchange = "user_exchange";

        // ORDER_CREATED
        var type = MessageType.ORDER_CREATED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated = new EventingBasicConsumer(_channel);
        consumerOrderCreated.Received += async (model, ea) =>
        {
            Console.WriteLine("Order created consumer");
            var _ = GetEventPayload<OrderCreatedEvent>(ea);
            var saga = new Saga();
            saga.IsMustBeReverted = false;
            saga.Billing = SagaStatus.PENDING;
            saga.Stock = SagaStatus.PENDING;
            saga.Delivery = SagaStatus.PENDING;
            var serialised = JsonSerializer.Serialize(saga);

            await _cache.SetStringAsync(_.OrderId.ToString(), serialised);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated);

        // BILLING
        type = MessageType.BILLING_ORDER_CONFIRMED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated2 = new EventingBasicConsumer(_channel);
        consumerOrderCreated2.Received += async (model, ea) =>
        {
            Console.WriteLine("Billing confirmed consumer");
            var order = GetEventPayload<BillingOrderConfirmedEvent>(ea);
            var saga = await GetSaga(order.OrderId);
            
            saga.Billing = SagaStatus.CONFIRMED;
            CheckForComplete(saga, order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated2);

        type = MessageType.BILLING_ORDER_REJECTED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated3 = new EventingBasicConsumer(_channel);
        consumerOrderCreated3.Received += async (model, ea) =>
        {
            Console.WriteLine("Billing rejected consumer");
            var order = GetEventPayload<BillingOrderRejectedEvent>(ea);
            var saga = await GetSaga(order.OrderId);
            saga.IsMustBeReverted = true;
            saga.Billing = SagaStatus.REVERTED;

            CheckForComplete(saga, order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated3);
        // EOF BILLING

        // STOCK
        type = MessageType.STOCK_ORDER_CONFIRMED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated4 = new EventingBasicConsumer(_channel);
        consumerOrderCreated4.Received += async (model, ea) =>
        {
            Console.WriteLine("Stock confirmed consumer");
            var order = GetEventPayload<StockOrderConfirmedEvent>(ea);
            var saga = await GetSaga(order.OrderId);
            saga.Stock = SagaStatus.CONFIRMED;
            if (saga.IsMustBeReverted)
            {
                saga.Stock = SagaStatus.REVERTED; //TEMPORARY
            }

            CheckForComplete(saga, order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated4);

        type = MessageType.STOCK_ORDER_REJECTED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated5 = new EventingBasicConsumer(_channel);
        consumerOrderCreated5.Received += async (model, ea) =>
        {
            Console.WriteLine("Stock rejected consumer");
            var order = GetEventPayload<StockOrderRejectedEvent>(ea);
            var saga = await GetSaga(order.OrderId);
            saga.IsMustBeReverted = true;
            saga.Stock = SagaStatus.REVERTED;
            CheckForComplete(saga, order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated5);
        // EOF STOCK

        // DELIVERY
        type = MessageType.DELIVERY_ORDER_CONFIRMED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated6 = new EventingBasicConsumer(_channel);
        consumerOrderCreated6.Received += async (model, ea) =>
        {
            Console.WriteLine("Delivery confirmed consumer");
            var order = GetEventPayload<DeliveryOrderConfirmedEvent>(ea);
            var saga = await GetSaga(order.OrderId);
            saga.Delivery = SagaStatus.CONFIRMED;
            if (saga.IsMustBeReverted)
            {
                saga.Delivery = SagaStatus.REVERTED; //TEMPORARY
            }

            CheckForComplete(saga, order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated6);

        type = MessageType.DELIVERY_ORDER_REJECTED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated7 = new EventingBasicConsumer(_channel);
        consumerOrderCreated7.Received += async (model, ea) =>
        {
            Console.WriteLine("Delivery rejected consumer");
            var order = GetEventPayload<DeliveryOrderRejectedEvent>(ea);
            var saga = await GetSaga(order.OrderId);
            saga.IsMustBeReverted = true;
            saga.Delivery = SagaStatus.REVERTED;
            CheckForComplete(saga, order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated7);
        // EOF DELIVERY
    }

    private T GetEventPayload<T>(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        return JsonSerializer.Deserialize<T>(message);
    }

    private void Send<T>(MessageType type, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = System.Text.Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "user_exchange", routingKey: $"Routing_key_{type}", basicProperties: null,
            body: body);
    }
    
    private async Task<Saga> GetSaga(int orderId)
    {
        Console.WriteLine($"Get from cache for orderId:: {orderId}");
        var cv = await _cache.GetStringAsync(orderId.ToString());
        return JsonSerializer.Deserialize<Saga>(cv);
    }

    private async Task CheckForComplete(Saga saga, int orderId)
    {
        bool isAllComplete = saga.Billing != SagaStatus.PENDING && saga.Delivery != SagaStatus.PENDING &&
                             saga.Stock != SagaStatus.PENDING;
        if (!isAllComplete)
        {
            Console.WriteLine("CheckforComplete:: continue");
            var c = JsonSerializer.Serialize<Saga>(saga, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine($"CheckforComplete:: continue {c}");

            await _cache.SetStringAsync(orderId.ToString(), c);
            return;
        }

        if (saga.IsMustBeReverted)
        {
            Send(MessageType.ORDER_REVERTED, new OrderRevertedEvent() { OrderId = orderId });
            Console.WriteLine($"Transaction error");
        }
        else
        {
            Console.WriteLine($"Transaction success");
            Send(MessageType.ORDER_CONFIRMED, new OrderConfirmedEvent() { OrderId = orderId });
        }

        await _cache.RemoveAsync(orderId.ToString());
    }
}