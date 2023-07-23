using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Commands.Players;
using BanHubData.Commands.Player;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player = BanHub.WebCore.Shared.Models.PlayersView.Player;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SignedInUsers _signedInUsers;

    public PlayerController(IMediator mediator, SignedInUsers signedInUsers)
    {
        _mediator = mediator;
        _signedInUsers = signedInUsers;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> CreateOrUpdatePlayerAsync([FromQuery] string authToken,
        [FromBody] CreateOrUpdatePlayerCommand request)
    {
        var id = await _mediator.Send(request);
        return Ok(id);
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayersPaginationAsync([FromBody] GetPlayersPaginationCommand playersPagination)
    {
        var result = await _mediator.Send(playersPagination);
        return Ok(result);
    }

    [HttpGet("Profile/{identity}")]
    public async Task<ActionResult<Player>> GetProfileAsync([FromRoute] string identity)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var instanceRoleAssigned = false;
        var webRoleAssigned = false;

        if (adminSignInGuid is not null)
        {
            instanceRoleAssigned = _signedInUsers.IsUserInAnyCommunityRole(adminSignInGuid, new[]
            {
                CommunityRole.Moderator,
                CommunityRole.Administrator,
                CommunityRole.SeniorAdmin,
                CommunityRole.Owner
            });
            webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
            {
                WebRole.Admin,
                WebRole.SuperAdmin
            });
        }

        var authorised = instanceRoleAssigned || webRoleAssigned;
        var result = await _mediator.Send(new GetProfileCommand {Identity = identity, Privileged = authorised});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("IsBanned")]
    public async Task<IActionResult> IsPlayerBannedAsync([FromBody] IsPlayerBannedCommand request)
    {
        var result = await _mediator.Send(request);
        return result ? Unauthorized() : Ok();
    }

    [HttpPost("GetToken/{identity}"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationTokenAsync([FromQuery] string authToken, [FromRoute] string identity)
    {
        var result = await _mediator.Send(new GetPlayerTokenCommand{Identity = identity});
        if (result is null) return NotFound();
        return Ok(result);
    }
    
    [HttpGet("Profile/Connections/{identity}")]
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Connection>>> GetProfileConnectionsAsync([FromRoute] string identity)
    {
        var result = await _mediator.Send(new GetProfileConnectionsCommand {Identity = identity});
        return Ok(result);
    }
}
