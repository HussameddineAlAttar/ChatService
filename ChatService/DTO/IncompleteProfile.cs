using System.ComponentModel.DataAnnotations;
namespace ChatService.DTO;

public record IncompleteProfile(
    [Required] string userName,
    [Required] string firstName,
    [Required] string lastName);