using System.Security.Claims;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntityController : ControllerBase
{
    private readonly IEntityService _entityService;

    public EntityController(IEntityService entityService)
    {
        _entityService = entityService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult> CreateOrUpdateAsync([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        return await _entityService.CreateOrUpdateAsync(request) switch
        {
            ControllerEnums.ReturnState.Updated => NoContent(),
            ControllerEnums.ReturnState.Created => StatusCode(StatusCodes.Status201Created),
            _ => BadRequest() // Should be unreachable
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<EntityDto>>> GetEntitiesAsync([FromBody] PaginationDto pagination)
    {
        return Ok(await _entityService.PaginationAsync(pagination));
    }

    [HttpGet]
    public async Task<ActionResult<EntityDto>> GetEntityAsync([FromQuery] string identity)
    {
        var privileged = User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin", "InstanceOwner", "WebAdmin",
            "WebSuperAdmin");
        var result = await _entityService.GetUserAsync(identity, privileged);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("Exists")]
    public async Task<ActionResult<bool>> EntityExistsAsync([FromQuery] string identity)
    {
        var result = await _entityService.HasEntityAsync(identity);
        if (!result) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("GetToken"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationTokenAsync([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        var result = await _entityService.GetAuthenticationTokenAsync(request);
        return Ok(result);
    }

    [HttpPost("AddNote")] // Authorised endpoint
    public async Task<ActionResult<NoteDto>> AddNoteAsync([FromBody] NoteDto request)
    {
        var privileged =
            User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin", "InstanceOwner", "WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var result = await _entityService.AddNoteAsync(request, adminIdentity);
        return result switch
        {
            true => Ok("Created note"),
            false => BadRequest("Failed to create note")
        };
    }
    
    [HttpPost("RemoveNote")] // Authorised endpoint
    public async Task<ActionResult<bool>> RemoveNoteAsync([FromBody] NoteDto request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var result = await _entityService.RemoveNoteAsync(request, adminIdentity);
        return result switch
        {
            true => Ok("Note deleted!"),
            false => BadRequest("Error removing note")
        };
    }
}
