namespace ChatService.Exceptions;

public class ProfileNotFoundException : Exception
{
    public List<string>? Usernames { get; }

    public ProfileNotFoundException(List<string> usernames) : base()
    {
        Usernames = usernames;
    }

    public ProfileNotFoundException(string message) : base(message)
    {
    }

    public ProfileNotFoundException() : base()
    {
    }
}
