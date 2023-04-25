using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record Conversation
{
    public Conversation([Required] List<string> Participants)
    {
        Participants.Sort();
        this.Participants = Participants;
        Id = string.Join("_", Participants);
        CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ModifiedTime = CreatedTime;
    }

    public List<string> Participants { get; init; }
    public string Id;
    public long CreatedTime;
    public long ModifiedTime;
}
