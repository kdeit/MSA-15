namespace OtusKdeBus.Model.Client;

public class DeliveryOrderRejectedEvent : BaseEvent
{
    public int OrderId { get; set; }

    public DeliveryOrderRejectedEvent() : base(MessageType.DELIVERY_ORDER_REJECTED)
    {
    }
}