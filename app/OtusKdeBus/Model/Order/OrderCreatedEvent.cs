namespace OtusKdeBus.Model.Client;

public class OrderCreatedEvent : BaseEvent
{
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public float Total { get; set; }

    public OrderCreatedEvent() : base(MessageType.ORDER_CREATED)
    {
    }
}