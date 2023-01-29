using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class ServerController : ControllerBase
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
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.Conflict => NoContent(),
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
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() 
        };
    }

}
