using BanHub.WebCore.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class StatisticsHub : Hub
{
    private readonly IStatisticService _statisticService;

    public StatisticsHub(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }

    public int GetCurrentOnlinePlayers() => _statisticService.GetOnlinePlayerCount();

    public int GetCurrentRecentBans() => _statisticService.GetRecentBansCount();
}
