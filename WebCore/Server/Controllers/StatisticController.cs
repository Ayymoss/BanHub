using BanHub.WebCore.Server.Interfaces;
using Data.Domains;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticController : ControllerBase
{
    private readonly IStatisticService _statisticService;

    public StatisticController(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }

    [HttpGet]
    public async Task<ActionResult<Statistic>> Statistics()
    {
        var result = await _statisticService.GetStatisticsAsync();
        return Ok(result);
    }
}
