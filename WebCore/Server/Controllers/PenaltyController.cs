using System.Security.Claims;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.DTOs;
using BanHub.WebCore.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class PenaltyController : ControllerBase
{
    private readonly IPenaltyService _penaltyService;

    public PenaltyController(IPenaltyService penaltyService)
    {
        _penaltyService = penaltyService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> AddPenalty([FromQuery] string authToken, [FromBody] PenaltyDto request)
    {
        var result = await _penaltyService.AddPenaltyAsync(request);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.BadRequest => BadRequest(),
            ControllerEnums.ReturnState.Conflict => Conflict("Infraction already exists"),
            ControllerEnums.ReturnState.NoContent => NoContent(),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("Evidence"), PluginAuthentication]
    public async Task<ActionResult<bool>> SubmitEvidence([FromQuery] string authToken, [FromBody] PenaltyDto request)
    {
        var result = await _penaltyService.SubmitEvidenceAsync(request);

        return result switch
        {
            true => Ok(true),
            false => BadRequest(false)
        };
    }

    [HttpGet]
    public async Task<ActionResult<InstanceDto>> GetPenalty([FromQuery] string guid)
    {
        var result = await _penaltyService.GetPenaltyAsync(guid);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.Ok => Ok(result.Item2),
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.BadRequest => BadRequest(),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetPenalties([FromBody] PaginationDto pagination)
    {
        return Ok(await _penaltyService.PaginationAsync(pagination));
    }

    [HttpGet("Index")]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetRecentPenalties()
    {
        return Ok(await _penaltyService.GetLatestThreeBansAsync());
    }

    [HttpPost("Remove")] // Authorised endpoint
    public async Task<ActionResult<bool>> RemovePenalty([FromBody] PenaltyDto request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");
        
        var result = await _penaltyService.RemovePenaltyAsync(request, adminIdentity);
        return result switch
        {
            true => Ok("Penalty deleted!"),
            false => BadRequest("Error removing penalty")
        };
    }
}
