using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record Profile
{
    public string Username { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string ProfilePictureId { get; set; }

    public Profile([Required] string Username, [Required] string FirstName,
        [Required] string LastName, [Required] string ProfilePictureId)
    {
        this.Username = Username;
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.ProfilePictureId = ProfilePictureId;
    }
}