namespace OtusKdeBus.Model.Client;

public class BillingOrderRejectedEvent : BaseEvent
{
    public int OrderId { get; set; }

    public BillingOrderRejectedEvent() : base(MessageType.BILLING_ORDER_REJECTED)
    {
    }
}