using BanHub.Application.Mediatr.Services.Commands.Statistics;
using BanHub.Domain.Interfaces.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.Infrastructure.SignalR.Hubs;

public class StatisticsHub(ISender sender) : Hub, IStatisticsHub
{
    public async Task<int> GetCurrentOnlinePlayers() => await sender.Send(new GetOnlinePlayersCommand());
    public async Task<int> GetCurrentRecentBans() => await sender.Send(new GetRecentBansCommand());
}
