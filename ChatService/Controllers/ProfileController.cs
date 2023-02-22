using Microsoft.AspNetCore.Mvc;
using ChatService.DTO;
using ChatService.Storage;

namespace ChatService.Controllers;


[ApiController]
[Route("profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileInterface profileInterface;
    private readonly IImageInterface blobStorage;

    public ProfileController(IProfileInterface _profileInterface, IImageInterface _imageInterface)
    {
        profileInterface = _profileInterface;
        blobStorage = _imageInterface;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        var profile = await profileInterface.GetProfile(username);
        if(profile == null)
        {
            return NotFound($"User with username {username} was not found.");
        }
        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(IncompleteProfile inProfile)
    {
        var existing = await profileInterface.GetProfile(inProfile.userName);
        if(existing != null) {
            return Conflict($"Cannot create profile. Username {inProfile.userName} is taken.");
        }
        Profile fullProfile = new Profile(inProfile.userName, inProfile.firstName, inProfile.lastName);
        await profileInterface.UpsertProfile(fullProfile);
        return CreatedAtAction(nameof(GetProfile), new { username = fullProfile.userName },fullProfile);

    }

    [HttpDelete("{username}")]
    public async Task<ActionResult<Profile>> DeleteProfile(string username)
    {
        var profile = await profileInterface.GetProfile(username);
        if (profile == null)
        {
            return NotFound($"A User with username {username} was not found");
        }
        await profileInterface.DeleteProfile(username);
        await blobStorage.DeleteImage(profile.ProfilePictureID);
        return Ok($"Profile of username {username} successfully deleted");
    }
}
