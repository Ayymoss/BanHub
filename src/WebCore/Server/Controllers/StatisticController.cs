using BanHub.Application.DTOs.WebView.Shared;
using BanHub.Application.Mediatr.Services.Commands.Statistics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Statistic>> Statistics()
    {
        var result = await sender.Send(new GetStatisticsCommand());
        return Ok(result);
    }
}
