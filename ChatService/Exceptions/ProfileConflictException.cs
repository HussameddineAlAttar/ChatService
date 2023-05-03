namespace ChatService.Exceptions;

public class ProfileConflictException : Exception
{
    public ProfileConflictException(string message = "") : base(message) { }
}
