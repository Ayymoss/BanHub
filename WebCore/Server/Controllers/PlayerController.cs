using System.Security.Claims;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Utilities;
using Data.Commands;
using Data.Commands.Player;
using Data.Domains;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<Player>>> GetEntitiesAsync([FromBody] Pagination pagination)
    {
        return Ok(await _playerService.PaginationAsync(pagination));
    }

    //[HttpGet] // Disabled - This needs to be rewritten using Mediator. The return value used to return Player.
    //public async Task<IActionResult> GetEntityAsync([FromQuery] string identity)
    //{
    //    var privileged = User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin",
    //        "InstanceOwner", "WebAdmin",
    //        "WebSuperAdmin");
    //    var result = await _entityService.GetUserAsync(identity, privileged);
    //    if (result is null) return NotFound();
    //    return Ok(result);
    //}

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

    [HttpPost("AddNote")] // Authorised endpoint
    public async Task<ActionResult<Note>> AddNoteAsync([FromBody] Note request)
    {
        var privileged = User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin", 
            "InstanceOwner", "WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var result = await _playerService.AddNoteAsync(request, adminIdentity);
        return result switch
        {
            true => Ok("Created note"),
            false => BadRequest("Failed to create note")
        };
    }

    [HttpPost("RemoveNote")] // Authorised endpoint
    public async Task<ActionResult<bool>> RemoveNoteAsync([FromBody] Note request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var result = await _playerService.RemoveNoteAsync(request, adminIdentity);
        return result switch
        {
            true => Ok("Note deleted!"),
            false => BadRequest("Error removing note")
        };
    }
}
