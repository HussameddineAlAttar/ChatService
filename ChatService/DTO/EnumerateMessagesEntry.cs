namespace ChatService.DTO;

public record EnumerateMessagesEntry
{
    public EnumerateMessagesEntry(string Text, string SenderUsername, long UnixTime)
    {
        this.Text = Text;
        this.SenderUsername = SenderUsername;
        this.UnixTime = UnixTime;
    }
    public string Text { get; }
    public string SenderUsername { get; }
    public long UnixTime { get; }
}
