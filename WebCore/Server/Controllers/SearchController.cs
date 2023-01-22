using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SearchDto>>> Search([FromQuery] string query)
    {
        if (query.Length < 3) return BadRequest();
        var result = await _searchService.Search(query);
        return Ok(result);
    }
}
