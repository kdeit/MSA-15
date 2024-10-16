namespace OtusKdeBus.Model.Client;

public class BillingOrderConfirmedEvent: BaseEvent
{
    public int OrderId { get; set; }
    
    public BillingOrderConfirmedEvent(): base(MessageType.BILLING_ORDER_CONFIRMED){}
}