using Microsoft.EntityFrameworkCore;
using OtusKdeBus;
using OtusKdeBus.Model.Client;

namespace OtusKdeDAL.BusConsumers;

public class OrderBusConsumer
{
    private IBusConsumer _consumer;
    private IBusProducer _producer;
    private BillingContext _cnt;

    public OrderBusConsumer(IBusConsumer busConsumer, BillingContext context, IBusProducer producer)
    {
        _consumer = busConsumer;
        _cnt = context;
        _producer = producer;
    }

    public void Init()
    {
        Action<OrderCreatedEvent> action = async (x) =>
        {
            var value = Convert.ToDecimal(x.Total);

            var debit = await _cnt.Wallets.Where(_ => _.UserId == x.UserId).Select(_ => _.Value).ToListAsync();
            var credit = await _cnt.Payments.Where(_ => _.UserId == x.UserId && _.Status == PaymentsStatus.ACCEPTED)
                .Select(_ => _.Value)
                .ToListAsync();
            var res = debit.Sum() - credit.Sum();

            if (res < value)
            {
                Console.WriteLine($"Rejected. Amount:: {res} Total:: {value}");
                _producer.SendMessage(new BillingOrderRejectedEvent() { OrderId = x.OrderId });
                return;
            }

            Console.WriteLine("Confirmed");
            _cnt.Payments.AddAsync(new Payments()
            {
                OrderId = x.OrderId,
                UserId = x.UserId,
                Value = Convert.ToDecimal(x.Total)
            });
            await _cnt.SaveChangesAsync();
            _producer.SendMessage(new BillingOrderConfirmedEvent() { OrderId = x.OrderId });
        };
        _consumer.OnOrderCreated("watermelon", action);

        Action<OrderRevertedEvent> action2 = async (x) =>
        {
            var payment = await _cnt.Payments.FirstOrDefaultAsync(_ => _.OrderId == x.OrderId);
            payment.Status = PaymentsStatus.CANCELED;
            await _cnt.SaveChangesAsync();
        };
        _consumer.OnOrderReverted("tomato", action2);
    }
}