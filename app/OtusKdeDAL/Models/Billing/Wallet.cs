namespace OtusKdeDAL;

public class Wallet : BaseEntity
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}