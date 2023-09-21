using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Players;
using BanHub.WebCore.Shared.Models.PlayersView;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Events.Player;
using BanHubData.Mediatr.Commands.Requests.Player;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController(IMediator mediator, ISignedInUsersManager signedInUsersManager) : ControllerBase
{
    [HttpPost, PluginAuthentication]
    public async Task<IActionResult> CreateOrUpdatePlayerAsync([FromBody] CreateOrUpdatePlayerNotification request)
    {
        await mediator.Publish(request);
        return Ok();
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<PaginationContext<Player>>> GetPlayersPaginationAsync(
        [FromBody] GetPlayersPaginationCommand playersPagination)
    {
        var result = await mediator.Send(playersPagination);
        return Ok(result);
    }

    [HttpGet("Profile/{identity}")]
    public async Task<ActionResult<Player>> GetProfileAsync([FromRoute] string identity)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var authorised = signedInUsersManager.IsUserInRole(adminSignInGuid, new[]
                         {
                             CommunityRole.Moderator,
                             CommunityRole.Administrator,
                             CommunityRole.SeniorAdmin,
                             CommunityRole.Owner
                         }, signedInUsersManager.IsUserInCommunityRole) ||
                         signedInUsersManager.IsUserInRole(adminSignInGuid, new[]
                         {
                             WebRole.Admin,
                             WebRole.SuperAdmin
                         }, signedInUsersManager.IsUserInWebRole);

        var result = await mediator.Send(new GetProfileCommand {Identity = identity, Privileged = authorised});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("IsBanned"), PluginAuthentication]
    public async Task<IActionResult> IsPlayerBannedAsync([FromBody] IsPlayerBannedCommand request)
    {
        var result = await mediator.Send(request);
        return result ? Unauthorized() : Ok();
    }

    [HttpGet("HasIdentityBan/{identity}")]
    public async Task<IActionResult> HasIdentityBanAsync([FromRoute] string identity)
    {
        var result = await mediator.Send(new HasIdentityBanCommand {Identity = identity});
        return result ? Unauthorized() : Ok();
    }

    [HttpGet("GetToken/{identity}"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationTokenAsync([FromRoute] string identity)
    {
        var result = await mediator.Send(new GetPlayerTokenCommand {Identity = identity});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("Profile/Connections")]
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Connection>>> GetProfileConnectionsAsync(
        [FromBody] GetProfileConnectionsPaginationCommand pagination)
    {
        var result = await mediator.Send(pagination);
        return Ok(result);
    }
}
