using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage.Interfaces;

public interface IProfileInterface
{
    Task CreateProfile(Profile profile);
    Task<Profile> GetProfile(string username);
    Task DeleteProfile(string username);
}
