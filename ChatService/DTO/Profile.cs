using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record Profile
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

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