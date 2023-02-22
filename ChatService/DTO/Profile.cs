using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record Profile
{
    public string userName { get; init; }
    public string firstName { get; init; }
    public string lastName { get; init; }
    public string ProfilePictureID { get; set; }

    public Profile(string userName, string firstName, string lastName)
    {
        this.userName = userName;
        this.firstName = firstName;
        this.lastName = lastName;
        ProfilePictureID = Guid.NewGuid().ToString();
    }
}