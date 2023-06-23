using ChatService.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record Conversation
{
    public Conversation([Required] List<string> Participants)
    {
        // sorting the participants for consistency
        Participants.Sort();
        this.Participants = Participants;
        Id = CreateConversationId(Participants);
        CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ModifiedTime = CreatedTime;
    }

    public string CreateConversationId(List<string> usernames)
    {
        usernames.Sort();
        return string.Join("+", usernames);
    }

    public List<string> Participants { get; init; }
    public string Id;
    public long CreatedTime;
    public long ModifiedTime;
}
