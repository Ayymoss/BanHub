using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class StatisticController : ControllerBase
{
    private readonly IStatisticService _statisticService;

    public StatisticController(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }

    [HttpGet]
    public async Task<ActionResult<StatisticDto>> Statistics()
    {
        var result = await _statisticService.GetStatistics();
        return Ok(result);
    }
}
