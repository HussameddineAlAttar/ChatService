using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record CreateConversationResponse
{
    public CreateConversationResponse([Required] string id, long CreatedUnixTime)
    {
        Id = id;
        this.CreatedUnixTime = CreatedUnixTime;
    }
    public long CreatedUnixTime { get; }
    public string Id { get; }
}
