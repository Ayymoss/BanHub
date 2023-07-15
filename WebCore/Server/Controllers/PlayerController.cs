using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Commands.Players;
using BanHubData.Commands.Player;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player = BanHub.WebCore.Shared.Models.PlayersView.Player;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlayerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> CreateOrUpdatePlayerAsync([FromQuery] string authToken,
        [FromBody] CreateOrUpdatePlayerCommand request)
    {
        var id = await _mediator.Send(request);
        return Ok(id);
    }

    [HttpPost("Players")]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayersPaginationAsync([FromBody] GetPlayersPaginationCommand playersPagination)
    {
        var result = await _mediator.Send(playersPagination);
        return Ok(result);
    }

    [HttpGet("Profile/{identity}")]
    public async Task<ActionResult<Player>> GetProfileAsync([FromRoute] string identity)
    {
        var result = await _mediator.Send(new GetProfileCommand {Identity = identity});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("IsBanned")]
    public async Task<IActionResult> IsPlayerBannedAsync([FromBody] IsPlayerBannedCommand request)
    {
        var result = await _mediator.Send(request);
        return result ? Forbid() : Ok();
    }

    [HttpPost("GetToken"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationTokenAsync([FromQuery] string authToken, [FromBody] GetPlayerTokenCommand request)
    {
        var result = await _mediator.Send(request);
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
