using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record CreateConvoResponse
{
    public CreateConvoResponse([Required] string id, long time)
    {
        Id = id;
        CreatedUnixTime = time;
    }
    public long CreatedUnixTime { get; }
    public string Id { get; }
}
