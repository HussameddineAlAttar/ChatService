namespace ChatService.Exceptions;

public class ProfileConflictException : Exception
{
    public ProfileConflictException() : base()
    {
    }
    public ProfileConflictException(string message) : base(message)
    {
    }
}
