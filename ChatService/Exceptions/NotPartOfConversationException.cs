namespace ChatService.Exceptions;

public class NotPartOfConversationException : Exception
{
    public NotPartOfConversationException(string message = "") : base(message) { }
}