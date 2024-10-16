namespace OtusKdeBus.Model.Client;

public class ClientUserCreatedEvent : BaseEvent
{
    public ClientUserCreatedEvent() : base(MessageType.USER_CREATED)
    {
    }

    public int UserId { get; set; }
}