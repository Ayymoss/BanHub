using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.DTOs;
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
        var result = await _penaltyService.AddPenalty(request);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            ControllerEnums.ProfileReturnState.NotModified => Conflict("Infraction already exists"),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("Evidence"), PluginAuthentication]
    public async Task<ActionResult<bool>> SubmitEvidence([FromQuery] string authToken, [FromBody] PenaltyDto request)
    {
        var result = await _penaltyService.SubmitEvidence(request);

        return result switch
        {
            true => Ok(true),
            false => BadRequest(false)
        };
    }

    [HttpGet]
    public async Task<ActionResult<InstanceDto>> GetPenalty([FromQuery] string guid)
    {
        var result = await _penaltyService.GetPenalty(guid);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet("All")]
    public async Task<ActionResult<InstanceDto>> GetPenalties()
    {
        var result = await _penaltyService.GetPenalties();
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() // Should never happen
        };
    }
    
    [HttpGet("Count")]
    public async Task<ActionResult<InstanceDto>> GetPenaltyDayCount()
    {
        var result = await _penaltyService.GetPenaltyDayCount();
        return Ok(result);
    }
}
