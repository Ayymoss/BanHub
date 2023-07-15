using System.Security.Claims;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SignedInUsers _signedInUsers;

    public NoteController(IMediator mediator, SignedInUsers signedInUsers)
    {
        _mediator = mediator;
        _signedInUsers = signedInUsers;
    }

    [HttpPost] // Authorised endpoint
    public async Task<IActionResult> AddNoteAsync([FromBody] AddNoteCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        if (adminSignInGuid is null) return Unauthorized("You are not authorised to perform this action");

        var instanceRoleAssigned = _signedInUsers.IsUserInAnyInstanceRole(adminSignInGuid, new[]
        {
            InstanceRole.InstanceModerator,
            InstanceRole.InstanceAdministrator,
            InstanceRole.InstanceSeniorAdmin,
            InstanceRole.InstanceOwner
        });
        var webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
        {
            WebRole.WebAdmin,
            WebRole.WebSuperAdmin
        });

        if (!instanceRoleAssigned && !webRoleAssigned) return Unauthorized("You are not authorised to perform this action");

        var result = await _mediator.Send(request);
        return result switch
        {
            true => Ok("Created note"),
            false => BadRequest("Failed to create note")
        };
    }

    [HttpDelete] // Authorised endpoint
    public async Task<IActionResult> DeleteNoteAsync([FromBody] DeleteNoteCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var adminNameIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminSignInGuid is null || adminName is null || adminNameIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var instanceRoleAssigned = _signedInUsers.IsUserInAnyInstanceRole(adminSignInGuid, new[]
        {
            InstanceRole.InstanceModerator,
            InstanceRole.InstanceAdministrator,
            InstanceRole.InstanceSeniorAdmin,
            InstanceRole.InstanceOwner
        });
        var webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
        {
            WebRole.WebAdmin,
            WebRole.WebSuperAdmin
        });

        if (!instanceRoleAssigned && !webRoleAssigned) return Unauthorized("You are not authorised to perform this action");

        request.ActionAdminUserName = adminName;
        request.ActionAdminIdentity = adminNameIdentity;
        var result = await _mediator.Send(request);
        if (!result) return BadRequest();
        return Ok();
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesAsync([FromRoute] string identity)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var instanceRoleAssigned = false;
        var webRoleAssigned = false;

        if (adminSignInGuid is not null)
        {
            instanceRoleAssigned = _signedInUsers.IsUserInAnyInstanceRole(adminSignInGuid, new[]
            {
                InstanceRole.InstanceModerator,
                InstanceRole.InstanceAdministrator,
                InstanceRole.InstanceSeniorAdmin,
                InstanceRole.InstanceOwner
            });
            webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
            {
                WebRole.WebAdmin,
                WebRole.WebSuperAdmin
            });
        }

        var authorised = instanceRoleAssigned || webRoleAssigned;
        var result = await _mediator.Send(new GetNotesCommand {Identity = identity, Authorised = authorised});
        return Ok(result);
    }
}
