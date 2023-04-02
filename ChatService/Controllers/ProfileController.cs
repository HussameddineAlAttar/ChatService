using Microsoft.AspNetCore.Mvc;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage.Interfaces;

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
            await blobStorage.DownloadImage(profile.ProfilePictureId);
            await profileInterface.CreateProfile(profile);
            return CreatedAtAction(nameof(GetProfile), new { username = profile.Username }, profile);
        }
        catch(Exception e)
        {
            if(e is ProfileConflictException)
            {
                return Conflict($"Cannot create profile. Username {profile.Username} is taken.");
            }
            else if(e is ImageNotFoundException)
            {
                return BadRequest($"Image with id {profile.ProfilePictureId} does not exist.");
            }
            throw;
        }     
    }
}
