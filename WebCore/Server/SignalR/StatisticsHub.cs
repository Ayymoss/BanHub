using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.SignalR;
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

    public async Task BroadcastOnlinePlayersAsync(int count)
    {
        await Clients.All.SendAsync(HubMethods.OnPlayerCountUpdate, count);
    }

    public async Task BroadcastRecentBansAsync(int count)
    {
        await Clients.All.SendAsync(HubMethods.OnRecentBansUpdate, count);
    }
}
