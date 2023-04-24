namespace ChatService.DTO;

public record SendMessageResponse
{
    public SendMessageResponse(long createdUnixTime)
    {
        CreatedUnixTime = createdUnixTime;
    }
    public long CreatedUnixTime {get ; }
}
