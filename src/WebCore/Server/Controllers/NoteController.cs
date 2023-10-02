using System.Security.Claims;
using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Interfaces;
using BanHub.Application.Mediatr.Note.Commands;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.ValueObjects.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController(ISender sender, ISignedInUsersManager signedInUsersManager) : ControllerBase
{
    [HttpPost] // Authorised endpoint
    public async Task<IActionResult> AddNoteAsync([FromBody] AddNoteCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminSignInGuid is null || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");

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

        if (!authorised) return Unauthorized("You are not authorised to perform this action");
        request.AdminIdentity = adminIdentity;
        var result = await sender.Send(request);
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

        if (!authorised) return Unauthorized("You are not authorised to perform this action");

        request.IssuerUserName = adminName;
        request.IssuerIdentity = adminNameIdentity;
        var result = await sender.Send(request);
        if (!result) return BadRequest();
        return Ok();
    }

    [HttpPost("Profile/Notes")]
    public async Task<ActionResult<PaginationContext<Note>>> GetProfileNotesPaginationAsync([FromBody] GetProfileNotesPaginationCommand pagination)
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

        pagination.Privileged = authorised;
        var result = await sender.Send(pagination);
        return Ok(result);
    }

    [HttpGet("NoteCount/{identity}")]
    public async Task<ActionResult<int>> GetUserNotesCountAsync([FromRoute] string identity)
    {
        var result = await sender.Send(new GetNoteCountCommand {Identity = identity});
        return Ok(result);
    }
}
