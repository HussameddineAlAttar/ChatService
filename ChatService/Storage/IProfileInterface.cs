using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage;

public interface IProfileInterface
{
    Task UpsertProfile(Profile profile);
    Task<Profile?> GetProfile(string username);
    Task DeleteProfile(string username);


}
