using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var result = await _serverService.AddAsync(request);
        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Conflict => NoContent(),
            ControllerEnums.ReturnState.Ok => Ok(),
            _ => BadRequest() 
        };
    }
    
    [HttpGet]
    public async Task<ActionResult<ServerDto>> Get([FromQuery] string serverId)
    {
        var result = await _serverService.GetAsync(serverId);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() 
        };
    }

}
