using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage;

public interface IProfileStore
{
    Task CreateProfile(Profile profile);
    Task<Profile> GetProfile(string username);
    Task<Profile> GetProfileByEmail(string email);
    Task DeleteProfile(string username);
}
