using Newtonsoft.Json;

namespace ChatService.DTO;

public record EnumConvoResponse
{
    public EnumConvoResponse(string id, long LastModifiedUnixTime, Profile Recipient)
    {
        Id = id;
        this.LastModifiedUnixTime = LastModifiedUnixTime;
        this.Recipient = Recipient;
    }
    public string Id { get; }
    public long LastModifiedUnixTime { get; }
    public Profile Recipient { get; }
}
