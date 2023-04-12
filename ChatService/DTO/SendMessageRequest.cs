using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record SendMessageRequest
{
    public SendMessageRequest([Required] string SenderUsername, [Required] string Text)
    {
        this.SenderUsername = SenderUsername;
        this.Text = Text;
        Id = Guid.NewGuid().ToString();
        Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        message = new Message(SenderUsername, Text, Id, Time);
    }
    public string Id;
    public string SenderUsername { get; init; }
    public string Text { get; init; }
    public long Time;
    public Message message;
}
