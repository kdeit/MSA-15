using System.Text;
using System.Text.Json;
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
    public bool IsMustBeReverted = false;
    public SagaStatus Billing = SagaStatus.PENDING;
    public SagaStatus Stock = SagaStatus.PENDING;
    public SagaStatus Delivery = SagaStatus.PENDING;
}

public class OrderTransactionSaga
{
    private IModel _channel;
    private Dictionary<int, Saga> _items = new Dictionary<int, Saga>();


    public OrderTransactionSaga()
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
        consumerOrderCreated.Received += (model, ea) =>
        {
            Console.WriteLine("Order created consumer");
            var _ = GetEventPayload<OrderCreatedEvent>(ea);
            _items[_.OrderId] = new Saga();
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated);

        // BILLING
        type = MessageType.BILLING_ORDER_CONFIRMED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated2 = new EventingBasicConsumer(_channel);
        consumerOrderCreated2.Received += (model, ea) =>
        {
            Console.WriteLine("Billing confirmed consumer");
            var order = GetEventPayload<BillingOrderConfirmedEvent>(ea);
            var saga = _items[order.OrderId];
            saga.Billing = SagaStatus.CONFIRMED;
            CheckForComplete(order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated2);

        type = MessageType.BILLING_ORDER_REJECTED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated3 = new EventingBasicConsumer(_channel);
        consumerOrderCreated3.Received += (model, ea) =>
        {
            Console.WriteLine("Billing rejected consumer");
            var order = GetEventPayload<BillingOrderRejectedEvent>(ea);
            var saga = _items[order.OrderId];
            saga.IsMustBeReverted = true;
            saga.Billing = SagaStatus.REVERTED;

            CheckForComplete(order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated3);
        // EOF BILLING

        // STOCK
        type = MessageType.STOCK_ORDER_CONFIRMED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated4 = new EventingBasicConsumer(_channel);
        consumerOrderCreated4.Received += (model, ea) =>
        {
            Console.WriteLine("Stock confirmed consumer");
            var order = GetEventPayload<StockOrderConfirmedEvent>(ea);
            var saga = _items[order.OrderId];
            saga.Stock = SagaStatus.CONFIRMED;
            if (saga.IsMustBeReverted)
            {
                saga.Stock = SagaStatus.REVERTED; //TEMPORARY
            }

            CheckForComplete(order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated4);

        type = MessageType.STOCK_ORDER_REJECTED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated5 = new EventingBasicConsumer(_channel);
        consumerOrderCreated5.Received += (model, ea) =>
        {
            Console.WriteLine("Stock rejected consumer");
            var order = GetEventPayload<StockOrderRejectedEvent>(ea);
            var saga = _items[order.OrderId];
            saga.IsMustBeReverted = true;
            saga.Stock = SagaStatus.REVERTED;
            CheckForComplete(order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated5);
        // EOF STOCK

        // DELIVERY
        type = MessageType.DELIVERY_ORDER_CONFIRMED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated6 = new EventingBasicConsumer(_channel);
        consumerOrderCreated6.Received += (model, ea) =>
        {
            Console.WriteLine("Delivery confirmed consumer");
            var order = GetEventPayload<DeliveryOrderConfirmedEvent>(ea);
            var saga = _items[order.OrderId];
            saga.Delivery = SagaStatus.CONFIRMED;
            if (saga.IsMustBeReverted)
            {
                saga.Delivery = SagaStatus.REVERTED; //TEMPORARY
            }

            CheckForComplete(order.OrderId);
        };
        _channel.BasicConsume($"Queue_{type}.{queue_name}", autoAck: true, consumerOrderCreated6);

        type = MessageType.DELIVERY_ORDER_REJECTED;
        _channel.QueueDeclare(queue: $"Queue_{type}.{queue_name}", durable: true, exclusive: false, autoDelete: false,
            arguments: null);
        _channel.QueueBind(queue: $"Queue_{type}.{queue_name}", exchange, routingKey: $"Routing_key_{type}");

        var consumerOrderCreated7 = new EventingBasicConsumer(_channel);
        consumerOrderCreated7.Received += (model, ea) =>
        {
            Console.WriteLine("Delivery rejected consumer");
            var order = GetEventPayload<DeliveryOrderRejectedEvent>(ea);
            var saga = _items[order.OrderId];
            saga.IsMustBeReverted = true;
            saga.Delivery = SagaStatus.REVERTED;
            CheckForComplete(order.OrderId);
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

    private void CheckForComplete(int orderId)
    {
        var saga = _items[orderId];
        bool isAllComplete = saga.Billing != SagaStatus.PENDING && saga.Delivery != SagaStatus.PENDING &&
                             saga.Stock != SagaStatus.PENDING;
        if (!isAllComplete)
        {
            Console.WriteLine("CheckforComplete:: continue");
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

        _items.Remove(orderId);
    }
}