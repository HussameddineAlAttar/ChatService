namespace ChatService.Exceptions;

public class MessageConflictException : Exception
{
    public MessageConflictException(string message = "") : base(message) { }
}
