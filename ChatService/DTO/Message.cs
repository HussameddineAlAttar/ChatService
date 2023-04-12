using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record Message
{
    public Message(string SenderUsername, string Text, string Id, long Time)
    {
        this.SenderUsername = SenderUsername;
        this.Text = Text;
        this.Id = Id;
        this.Time = Time;
    }

    public string SenderUsername { get; init; }
    public string Text { get; init; }
    public string Id { get; set; }
    public long Time { get; set; }
}
