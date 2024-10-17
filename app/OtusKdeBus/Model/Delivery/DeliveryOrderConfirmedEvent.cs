namespace OtusKdeBus.Model.Client;

public class DeliveryOrderConfirmedEvent: BaseEvent
{
    public int OrderId { get; set; }
    
    public DeliveryOrderConfirmedEvent(): base(MessageType.DELIVERY_ORDER_CONFIRMED){}
}