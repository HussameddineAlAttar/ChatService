using ChatService.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record Conversation
{
    public Conversation([Required] List<string> Participants)
    {
        this.Participants = Participants;
        Id = Participants.JoinToConversationId();
        CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ModifiedTime = CreatedTime;
    }

    public List<string> Participants { get; init; }
    public string Id;
    public long CreatedTime;
    public long ModifiedTime;
}
