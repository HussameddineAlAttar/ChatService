using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record Profile
{
    public string Username { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string ProfilePictureId { get; set; }

    public Profile(string Username, string FirstName, string LastName, string ProfilePictureId)
    {
        this.Username = Username;
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.ProfilePictureId = ProfilePictureId;
    }
}