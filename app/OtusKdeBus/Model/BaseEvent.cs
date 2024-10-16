namespace OtusKdeBus.Model;

public class BaseEvent
{
    private MessageType _type;

    public BaseEvent(MessageType type)
    {
        _type = type;
    }

    public MessageType GetEventType()
    {
        return _type;
    }
}