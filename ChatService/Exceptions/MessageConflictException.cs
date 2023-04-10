namespace ChatService.Exceptions;

public class MessageConflictException : Exception
{
    public MessageConflictException() : base()
    {
    }
    public MessageConflictException(string message) : base(message)
    {
    }
}
