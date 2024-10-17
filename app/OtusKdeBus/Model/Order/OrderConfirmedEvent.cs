namespace OtusKdeBus.Model.Client;

public class OrderConfirmedEvent : BaseEvent
{
    public int OrderId { get; set; }

    public OrderConfirmedEvent() : base(MessageType.ORDER_CONFIRMED)
    {
    }
}