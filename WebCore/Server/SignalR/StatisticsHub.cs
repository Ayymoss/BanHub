using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class StatisticsHub(ISender sender) : Hub
{
    public async Task<int> GetCurrentOnlinePlayers() => await sender.Send(new GetOnlinePlayersCommand());
    public async Task<int> GetCurrentRecentBans() => await sender.Send(new GetRecentBansCommand());
}
