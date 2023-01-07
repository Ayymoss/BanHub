using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class ServerController : Controller
{
    private readonly IServerService _serverService;


    public ServerController(IServerService serverService)
    {
        _serverService = serverService;
    }
    
    [HttpPost, PluginAuthentication]
    public async Task<ActionResult> Add([FromQuery] string authToken, [FromBody] ServerDto request)
    {
        var result = await _serverService.Add(request);
        return result switch
        {
            ControllerEnums.ProfileReturnState.NotFound => StatusCode(404),
            ControllerEnums.ProfileReturnState.Conflict => StatusCode(409),
            ControllerEnums.ProfileReturnState.Ok => Ok(),
            _ => BadRequest() 
        };
    }
    
    [HttpGet]
    public async Task<ActionResult<ServerDto>> Get([FromQuery] string serverId)
    {
        var result = await _serverService.Get(serverId);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.NotFound => StatusCode(404),
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() 
        };
    }

}
