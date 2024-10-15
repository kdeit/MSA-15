namespace OtusKdeDAL;

public class Order : BaseEntity
{
    public int UserId { get; set; }
    public float Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
}

public enum OrderStatus
{
    CREATED,
    CONFIRMED,
    REJECTED
}