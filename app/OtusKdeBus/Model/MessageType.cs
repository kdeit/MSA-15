namespace OtusKdeBus.Model;

public enum MessageType
{
    USER_CREATED = 0,
    ORDER_CREATED,
    ORDER_REVERTED,
    ORDER_CONFIRMED,
    BILLING_ORDER_CONFIRMED,
    BILLING_ORDER_REJECTED,
    STOCK_ORDER_CONFIRMED,
    STOCK_ORDER_REJECTED,
    DELIVERY_ORDER_CONFIRMED,
    DELIVERY_ORDER_REJECTED
}