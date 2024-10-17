namespace OtusKdeBus.Model.Client;

public class StockOrderConfirmedEvent: BaseEvent
{
    public int OrderId { get; set; }
    
    public StockOrderConfirmedEvent(): base(MessageType.STOCK_ORDER_CONFIRMED){}
}