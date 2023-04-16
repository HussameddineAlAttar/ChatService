using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record ConversationResponse
{
    public ConversationResponse(string id, long time, Profile Recipient)
    {
        Id = id;
        LastModifiedUnixTime = time;
        this.Recipient = Recipient;
    }
    public string Id { get; }
    public long LastModifiedUnixTime { get; }
    public Profile Recipient { get; }
}
