namespace ChatService.Exceptions;

public class ConversationConflictException : Exception
{
    public ConversationConflictException() : base()
    {
    }
    public ConversationConflictException(string message) : base(message)
    {
    }
}