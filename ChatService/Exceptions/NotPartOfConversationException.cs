namespace ChatService.Exceptions;

public class NotPartOfConversationException : Exception
{
    public NotPartOfConversationException() : base()
    {
    }
    public NotPartOfConversationException(string message) : base(message)
    {
    }
}