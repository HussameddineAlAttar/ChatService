namespace ChatService.DTO;

public class MessageTokenResponse
{
    public MessageTokenResponse(List<EnumMessageResponse> Messages, string token)
    {
        this.Messages = Messages;
        continuationToken = token;
    }
    public List<EnumMessageResponse> Messages { get; }
    public string continuationToken { get; }
}
