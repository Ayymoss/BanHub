using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class EntityController : ControllerBase
{
    private readonly IEntityService _entityService;

    public EntityController(IEntityService entityService)
    {
        _entityService = entityService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult> CreateOrUpdate([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        return await _entityService.CreateOrUpdate(request) switch
        {
            ControllerEnums.ProfileReturnState.Updated => NoContent(),
            ControllerEnums.ProfileReturnState.Created => StatusCode(StatusCodes.Status201Created),
            _ => BadRequest() // Should be unreachable
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<EntityDto>>> GetEntities([FromBody] PaginationDto pagination)
    {
        return Ok(await _entityService.Pagination(pagination));
    }

    [HttpGet]
    public async Task<ActionResult<EntityDto>> GetEntity([FromQuery] string identity)
    {
        var result = await _entityService.GetUser(identity);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("Exists")]
    public async Task<ActionResult<bool>> EntityExists([FromQuery] string identity)
    {
        var result = await _entityService.HasEntity(identity);
        if (!result) return NotFound(result);
        return Ok(result);
    }
    
    [HttpGet("Online")]
    public async Task<ActionResult<int>> GetOnlineCount()
    {
        var result = await _entityService.GetOnlineCount();
        return Ok(result);
    }
    
    [HttpPost("GetToken"), PluginAuthentication]
    public async Task<ActionResult<string>> GetAuthenticationToken([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        var result = await _entityService.GetAuthenticationToken(request);
        return Ok(result);
    }
    
}
