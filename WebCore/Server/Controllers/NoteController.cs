using System.Security.Claims;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Note;
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

        request.IssuerUserName = adminName;
        request.IssuerIdentity = adminNameIdentity;
        var result = await _mediator.Send(request);
        if (!result) return BadRequest();
        return Ok();
    }

    [HttpPost("Profile/Notes")]
    public async Task<ActionResult<PaginationContext<Note>>> GetProfileNotesPaginationAsync([FromBody] GetProfileNotesPaginationCommand pagination)
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

        pagination.Privileged = authorised;
        var result = await _mediator.Send(pagination);
        return Ok(result);
    }

    [HttpGet("NoteCount/{identity}")]
    public async Task<ActionResult<int>> GetUserNotesCountAsync([FromRoute] string identity)
    {
        var result = await _mediator.Send(new GetNoteCountCommand {Identity = identity});
        return Ok(result);
    }
}
