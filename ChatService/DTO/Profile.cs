using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record Profile
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }

    public Profile([Required] string Username, [Required] string Email, [Required] string Password,
        [Required] string FirstName, [Required] string LastName)
    {
        this.Username = Username;
        this.Email = Email;
        this.Password = Password;
        this.FirstName = FirstName;
        this.LastName = LastName;
    }
}