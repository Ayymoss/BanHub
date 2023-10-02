using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Interfaces;
using BanHub.Application.Mediatr.Community.Commands;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.ValueObjects.Plugin;
using BanHub.Domain.ValueObjects.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Community = BanHub.Application.DTOs.WebView.CommunityProfileView.Community;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunityController(ISender sender, ISignedInUsersManager signedInUsersManager) : ControllerBase
{
    /// <summary>
    /// Creates or Updates an instance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateCommunityAsync([FromBody] CreateOrUpdateCommunityCommand request)
    {
        var headerIp = HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var header)
            ? header.ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString();

        request.HeaderIp = headerIp;
        var result = await sender.Send(request);

        return result switch
        {
            ControllerEnums.ReturnState.Created => StatusCode(StatusCodes.Status201Created), // New, added
            ControllerEnums.ReturnState.Conflict => Conflict(), // Conflicting GUIDs
            ControllerEnums.ReturnState.Ok => Ok(), // Not activated
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet("Active/{identity}")]
    public async Task<ActionResult> IsCommunityActiveAsync([FromRoute] string identity)
    {
        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await sender.Send(new IsCommunityActiveCommand {CommunityGuid = guid});
        if (!result) return Unauthorized();
        return Accepted();
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<Community>> GetCommunity([FromRoute] string identity)
    {
        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await sender.Send(new GetCommunityCommand {CommunityGuid = guid});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<PaginationContext<Community>>> GetCommunityAsync(
        [FromBody] GetCommunitiesPaginationCommand pagination)
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

    [HttpPost("Profile/Servers")]
    public async Task<ActionResult<IEnumerable<Community>>> GetCommunityProfileServersAsync(
        [FromBody] GetCommunityProfileServersPaginationCommand identity)
    {
        var result = await sender.Send(identity);
        return Ok(result);
    }

    [HttpPatch("Activation/{identity}")]
    public async Task<IActionResult> ActivateCommunityAsync([FromRoute] string identity)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        if (adminSignInGuid is null) return Unauthorized("You are not authorised to perform this action");

        var authorised = signedInUsersManager.IsUserInRole(adminSignInGuid, new[] {WebRole.SuperAdmin}, signedInUsersManager.IsUserInWebRole);
        if (!authorised) return Unauthorized("You are not authorised to perform this action");

        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await sender.Send(new ToggleCommunityActivationCommand {CommunityGuid = guid});

        if (!result) return NotFound();
        return Ok();
    }

    [HttpPost("Profile/Penalties")]
    public async Task<ActionResult<IEnumerable<Penalty>>> GetCommunityPenaltiesAsync(
        [FromBody] GetCommunityProfilePenaltiesPaginationCommand pagination)
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

        var result = await sender.Send(pagination);
        return Ok(result);
    }
}
