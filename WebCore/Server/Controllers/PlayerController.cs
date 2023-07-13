using System.Security.Claims;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Commands.Players;
using BanHub.WebCore.Shared.Utilities;
using BanHubData.Commands.Player;
using BanHubData.Domains;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player = BanHub.WebCore.Shared.Models.PlayersView.Player;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IMediator _mediator;

    public PlayerController(IPlayerService playerService, IMediator mediator)
    {
        _playerService = playerService;
        _mediator = mediator;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> CreateOrUpdateAsync([FromQuery] string authToken, [FromBody] CreateOrUpdatePlayerCommand request)
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
    public async Task<ActionResult<Player>> GetProfileAsync([FromQuery] string identity)
    {
        var result = await _mediator.Send(new GetProfileCommand{Identity = identity});
        return Ok(result);
    }
    

    [HttpGet] // Disabled - This needs to be rewritten using Mediator. The return value used to return Player.
    public async Task<IActionResult> GetEntityAsync([FromQuery] string identity)
    {
        var privileged = User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin",
            "InstanceOwner", "WebAdmin",
            "WebSuperAdmin");
        var result = await _playerService.GetUserAsync(identity, privileged);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("IsBanned")]
    public async Task<IActionResult> IsPlayerBannedAsync([FromBody] IsPlayerBannedCommand request)
    {
        var result = await _mediator.Send(request);   
        return result ? Forbid() : Ok();
    }

    [HttpGet("Exists")]
    public async Task<ActionResult<bool>> EntityExistsAsync([FromQuery] string identity)
    {
        var result = await _playerService.HasEntityAsync(identity);
        if (!result) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("GetToken"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationTokenAsync([FromQuery] string authToken, [FromBody] GetPlayerTokenCommand request)
    {
        var result = await _mediator.Send(request);
        if (result is null) return NotFound();
        return Ok(result);
    }

}
