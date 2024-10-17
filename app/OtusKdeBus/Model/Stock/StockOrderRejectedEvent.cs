namespace OtusKdeBus.Model.Client;

public class StockOrderRejectedEvent: BaseEvent
{
    public int OrderId { get; set; }
    
    public StockOrderRejectedEvent(): base(MessageType.STOCK_ORDER_REJECTED){}
}