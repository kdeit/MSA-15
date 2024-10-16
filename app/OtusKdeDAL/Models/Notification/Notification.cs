using System.ComponentModel.DataAnnotations;

namespace OtusKdeDAL;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    [EmailAddress] public string Email { get; set; }
    public NotificationStatus Status { get; set; }
    public string Message { get; set; }
    
}

public enum NotificationType
{
    ORDER_CONFIRMED,
    ORDER_REJECTED
}

public enum NotificationStatus
{
    SENDED,
    ERROR
}