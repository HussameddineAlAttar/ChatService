using Microsoft.AspNetCore.Mvc;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;

namespace ChatService.Controllers;


[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileStore profileInterface;

    public ProfileController(IProfileStore _profileInterface)
    {
        profileInterface = _profileInterface;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        try
        {
            var profile = await profileInterface.GetProfile(username);
            return Ok(profile);
        }
        catch(Exception e)
        {
            if(e is ProfileNotFoundException)
            {
                return NotFound($"Profile with username {username} not found.");
            }
            throw;
        }
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        try
        {
            await profileInterface.CreateProfile(profile);
            return CreatedAtAction(nameof(GetProfile), new { username = profile.Username }, profile);
        }
        catch(Exception e)
        {
            if(e is ProfileConflictException)
            {
                return Conflict($"Cannot create profile. Username {profile.Username} is taken.");
            }
            throw;
        }     
    }
}
