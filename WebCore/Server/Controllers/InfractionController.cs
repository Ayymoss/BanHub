using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfractionController : Controller
{
    private readonly IInfractionService _infractionService;

    public InfractionController(IInfractionService infractionService)
    {
        _infractionService = infractionService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> AddInfraction([FromQuery] string authToken, [FromBody] InfractionDto request)
    {
        var result = await _infractionService.AddInfraction(request);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            ControllerEnums.ProfileReturnState.NotModified => StatusCode(304, "Infraction already exists"),
            _ => BadRequest() // Should never happen
        };
    }
    
    [HttpGet]
    public async Task<ActionResult<InstanceDto>> GetInfraction([FromQuery] string guid)
    {
        var result = await _infractionService.GetInfraction(guid);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            _ => BadRequest() // Should never happen
        };
    }
    
    [HttpGet("All")]
    public async Task<ActionResult<InstanceDto>> GetInfractions()
    {
        var result = await _infractionService.GetInfractions();
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            _ => BadRequest() // Should never happen
        };
    }
    
    [HttpGet("Count")]
    public async Task<ActionResult<int>> GetInfractionCount()
    {
        return Ok(await _infractionService.GetInfractionCount());
    }
}
