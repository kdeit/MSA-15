using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtusKdeDAL.BusConsumers;

public class BillingBusConsumer
{
    private IBusConsumer _consumer;
    private NotificationContext _cnt;
    private readonly HttpClient _http;
    private readonly HttpClient _http2;
    private readonly IHostEnvironment _env;

    public BillingBusConsumer(
        IBusConsumer busConsumer,
        NotificationContext context,
        HttpClient http,
        HttpClient http2,
        IHostEnvironment env
    )
    {
        _consumer = busConsumer;
        _cnt = context;
        _http = http;
        _http2 = http2;
        _env = env;
        _http.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:5015")
            : new Uri("http://api-client-asp.default.svc.cluster.local");
        _http2.BaseAddress = _env.IsDevelopment()
            ? new Uri("http://localhost:5225")
            : new Uri("http://api-order-asp.default.svc.cluster.local");
    }

    public void Init()
    {
        Action<BillingOrderConfirmedEvent> action = async (x) =>
        {
            Console.WriteLine("Hello");
            var order = await GetOrderById(x.OrderId);
            if (order is null) return;
            var user = await GetUserById(order.UserId);
            if (user is null) return;

            var nv = new Notification()
            {
                Email = user.Email,
                Status = NotificationStatus.SENDED,
                UserId = user.Id,
                Message = $"Order with id {order.Id} confirmed"
            };
            _cnt.Add(nv);
            _cnt.SaveChanges();
        };
        _consumer.OnBillingOrderConfirmed("apple", action);

        Action<BillingOrderRejectedEvent> action2 = async (x) =>
        {
            var order = await GetOrderById(x.OrderId);
            if (order is null) return;
            var user = await GetUserById(order.UserId);
            if (user is null) return;

            var nv = new Notification()
            {
                Email = user.Email,
                Status = NotificationStatus.SENDED,
                UserId = user.Id,
                Message = $"Order with id {order.Id} rejected"
            };
            _cnt.Add(nv);
            _cnt.SaveChanges();
        };
        _consumer.OnBillingOrderRejected("pear", action2);
    }

    private async Task<Order?> GetOrderById(int id)
    {
        var res0 = await _http2.GetAsync($"order/{id}");
        if (!res0.IsSuccessStatusCode) return null;
        return await res0.Content.ReadFromJsonAsync<Order>();
    }

    private async Task<User?> GetUserById(int id)
    {
        var res0 = await _http.GetAsync($"user/id/{id}");
        if (!res0.IsSuccessStatusCode) return null;
        return await res0.Content.ReadFromJsonAsync<User>();
    }
}