namespace OtusKdeDAL;

public enum PaymentsStatus
{
    ACCEPTED,
    CANCELED
}

public class Payments : BaseEntity
{
    
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public decimal Value { get; set; }
    public PaymentsStatus Status { get; set; } = PaymentsStatus.ACCEPTED;
}