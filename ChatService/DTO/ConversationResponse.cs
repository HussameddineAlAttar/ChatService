using System.ComponentModel.DataAnnotations;

namespace ChatService.DTO;

public record ConversationResponse
{
    public ConversationResponse(string id, long time, List<Profile> profiles)
    {
        Id = id;
        LastModifiedUnixTime = time;
        Recepients = profiles;
    }
    public string Id { get; }
    public long LastModifiedUnixTime { get; }
    public List<Profile> Recepients { get; }
}
