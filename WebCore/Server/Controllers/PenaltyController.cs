using System.Security.Claims;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PenaltyController(ISender sender, ISignedInUsersManager signedInUsersManager) : ControllerBase
{
    [HttpPost, PluginAuthentication]
    public async Task<IActionResult> AddPlayerPenaltyAsync([FromBody] AddPlayerPenaltyCommand request)
    {
        var result = await sender.Send(request);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Ok => Ok(),
            ControllerEnums.ReturnState.Conflict => Conflict(),
            _ => BadRequest()
        };
    }

    [HttpPatch("Evidence"), PluginAuthentication]
    public async Task<IActionResult> AddPlayerPenaltyEvidenceAsync([FromBody] AddPlayerPenaltyEvidenceCommand request) =>
        await HandleAddPlayerPenaltyEvidence(request);

    [HttpPatch("WebEvidence")]
    public async Task<IActionResult> AddPlayerPenaltyWebEvidenceAsync([FromBody] AddPlayerPenaltyEvidenceCommand request)
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

        request.IssuerUsername = adminName;
        request.IssuerIdentity = adminNameIdentity;
        return await HandleAddPlayerPenaltyEvidence(request);
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<PaginationContext<Penalty>>> GetPenaltiesPaginationAsync(
        [FromBody] GetPenaltiesPaginationCommand penaltiesPagination)
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

        penaltiesPagination.Privileged = authorised;
        var result = await sender.Send(penaltiesPagination);
        return Ok(result);
    }

    [HttpPost("Profile/Penalties")]
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Penalty>>> GetProfilePenaltiesPaginationAsync(
        [FromBody] GetProfilePenaltiesPaginationCommand penaltiesPagination)
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

        penaltiesPagination.Privileged = authorised;
        var result = await sender.Send(penaltiesPagination);
        return Ok(result);
    }

    

    [HttpGet("Latest")]
    public async Task<ActionResult<IEnumerable<Shared.Models.IndexView.Penalty>>> GetLatestBansAsync()
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

        var result = await sender.Send(new GetLatestBansCommand
        {
            Privileged = authorised
        });
        return Ok(result);
    }

    [HttpDelete("Delete")] // Authorised endpoint
    public async Task<IActionResult> RemovePenaltyAsync([FromBody] RemovePenaltyCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var adminNameIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminName is null || adminSignInGuid is null || adminNameIdentity is null)
            return Unauthorized("You are not authorised to perform this action");

        var authorised = signedInUsersManager.IsUserInRole(adminSignInGuid, new[]
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

    // Inline handler
    private async Task<IActionResult> HandleAddPlayerPenaltyEvidence(AddPlayerPenaltyEvidenceCommand request)
    {
        var result = await sender.Send(request);

        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Conflict => Conflict(),
            _ => Ok()
        };
    }
}
