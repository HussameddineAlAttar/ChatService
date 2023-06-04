using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record Profile
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string? ProfilePictureId { get; set; }

    public Profile([Required] string Username, [Required] string Email, [Required] string FirstName,
        [Required] string LastName, string? ProfilePictureId = "")
    {
        this.Username = Username;
        this.Email = Email;
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.ProfilePictureId = ProfilePictureId;
    }
}