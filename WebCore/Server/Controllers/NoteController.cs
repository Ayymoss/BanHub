using System.Security.Claims;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Utilities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly IMediator _mediator;

    public NoteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost] // Authorised endpoint
    public async Task<IActionResult> AddNoteAsync([FromBody] AddNoteCommand request)
    {
        var privileged = User.IsInAnyRole("InstanceModerator", "InstanceAdministrator", "InstanceSeniorAdmin",
            "InstanceOwner", "WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var result = await _mediator.Send(request);
        return result switch
        {
            true => Ok("Created note"),
            false => BadRequest("Failed to create note")
        };
    }

    [HttpDelete] // Authorised endpoint
    public async Task<IActionResult> RemoveNoteAsync([FromBody] DeleteNoteCommand request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var result = await _mediator.Send(request);
        return result switch
        {
            true => Ok("Note deleted!"),
            false => BadRequest("Error removing note")
        };
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesAsync([FromQuery] string identity)
    {
        var result = await _mediator.Send(new GetNotesCommand {Identity = identity});
        return Ok(result);
    }
}
