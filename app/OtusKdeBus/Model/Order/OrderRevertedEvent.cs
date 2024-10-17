namespace OtusKdeBus.Model.Client;

public class OrderRevertedEvent : BaseEvent
{
    public int OrderId { get; set; }

    public OrderRevertedEvent() : base(MessageType.ORDER_REVERTED)
    {
    }
}