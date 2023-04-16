namespace ChatService.DTO;

public record SendMessageResponse
{
    public SendMessageResponse(long CreatedUnixTime)
    {
        this.CreatedUnixTime = CreatedUnixTime;
    }
    public long CreatedUnixTime {get ; set; }
}
