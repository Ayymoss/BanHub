using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : Controller
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    //[Authorize(Policy = "InstanceAPIKeyAuth")]
    [HttpPost]
    public async Task<ActionResult> CreateOrUpdate([FromBody] ProfileDto request)
    {
        return await _profileService.CreateOrUpdate(request) switch
        {
            ControllerEnums.ProfileReturnState.Updated => NoContent(),
            ControllerEnums.ProfileReturnState.Created => StatusCode(StatusCodes.Status201Created),
            _ => BadRequest() // Should be unreachable
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfileDto>>> GetUsers()
    {
        return Ok(await _profileService.GetUsers());
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<ProfileDto>> GetUser(string identity)
    {
        var result = await _profileService.GetUser(identity);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
