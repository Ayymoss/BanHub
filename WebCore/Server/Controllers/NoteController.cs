using System.Security.Claims;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Commands.Note;
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
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminSignInGuid is null || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

        var authorised = SignedInUsers.IsUserInRole(adminSignInGuid, new[]
                         {
                             CommunityRole.Moderator,
                             CommunityRole.Administrator,
                             CommunityRole.SeniorAdmin,
                             CommunityRole.Owner
                         }, _signedInUsers.IsUserInCommunityRole) ||
                         SignedInUsers.IsUserInRole(adminSignInGuid, new[]
                         {
                             WebRole.Admin,
                             WebRole.SuperAdmin
                         }, _signedInUsers.IsUserInWebRole);

        if (!authorised) return Unauthorized("You are not authorised to perform this action");
        request.AdminIdentity = adminIdentity;
        var result = await _mediator.Send(request);
        if (!result) return BadRequest();
        return Ok();
    }

    [HttpDelete] // Authorised endpoint
    public async Task<IActionResult> DeleteNoteAsync([FromBody] DeleteNoteCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var adminNameIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminSignInGuid is null || adminName is null || adminNameIdentity is null)
            return Unauthorized("You are not authorised to perform this action");

        var authorised = SignedInUsers.IsUserInRole(adminSignInGuid, new[]
                         {
                             CommunityRole.Moderator,
                             CommunityRole.Administrator,
                             CommunityRole.SeniorAdmin,
                             CommunityRole.Owner
                         }, _signedInUsers.IsUserInCommunityRole) ||
                         SignedInUsers.IsUserInRole(adminSignInGuid, new[]
                         {
                             WebRole.Admin,
                             WebRole.SuperAdmin
                         }, _signedInUsers.IsUserInWebRole);

        if (!authorised) return Unauthorized("You are not authorised to perform this action");

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

        var authorised = SignedInUsers.IsUserInRole(adminSignInGuid, new[]
                         {
                             CommunityRole.Moderator,
                             CommunityRole.Administrator,
                             CommunityRole.SeniorAdmin,
                             CommunityRole.Owner
                         }, _signedInUsers.IsUserInCommunityRole) ||
                         SignedInUsers.IsUserInRole(adminSignInGuid, new[]
                         {
                             WebRole.Admin,
                             WebRole.SuperAdmin
                         }, _signedInUsers.IsUserInWebRole);

        var result = await _mediator.Send(new GetNotesCommand {Identity = identity, Authorised = authorised});
        return Ok(result);
    }

    [HttpGet("NoteCount/{identity}")]
    public async Task<ActionResult<int>> GetUserNotesCountAsync([FromRoute] string identity)
    {
        var result = await _mediator.Send(new GetNoteCountCommand {Identity = identity});
        return Ok(result);
    }
}
