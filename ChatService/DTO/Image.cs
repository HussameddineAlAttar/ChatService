namespace ChatService.DTO;

public record Image
{
    public Stream? Content { get; set; }
    public string? ContentType { get; set; }

    public Image(Stream? content, string? contentType)
    {
        Content = content;
        if(contentType == "image/jpg")
        {
            contentType = "image/jpeg";
        }
        ContentType = contentType;
    }
}
