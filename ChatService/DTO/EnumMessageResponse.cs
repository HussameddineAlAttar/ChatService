namespace ChatService.DTO;

public record EnumMessageResponse
{
    public EnumMessageResponse(string Text, string SenderUsername, long UnixTime)
    {
        this.Text = Text;
        this.SenderUsername = SenderUsername;
        this.UnixTime = UnixTime;
    }
    public string Text { get; }
    public string SenderUsername { get; }
    public long UnixTime { get; }
}
