namespace ChatService.Exceptions;

public class ConversationNotFoundException : Exception
{
    public ConversationNotFoundException() : base()
    {
    }
    public ConversationNotFoundException(string message) : base(message)
    {
    }
}