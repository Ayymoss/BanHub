using BanHub.Application.DTOs.WebView.Shared;
using BanHub.Application.Interfaces;
using BanHub.Application.Mediatr.Server.Commands;
using BanHub.Application.Mediatr.Services.Commands.Statistics;
using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Services.Commands;

public class StatisticHandler(IStatisticsCache statisticsCache, IPublisher publisher) : IRequestHandler<GetStatisticsCommand, Statistic>,
    IRequestHandler<GetOnlinePlayersCommand, int>,
    IRequestHandler<GetRecentBansCommand, int>, IRequestHandler<GetServerOnlineCountsCommand, Dictionary<string, int>>
{
    public async Task<Statistic> Handle(GetStatisticsCommand request, CancellationToken cancellationToken)
    {
        if (!statisticsCache.Loaded) await publisher.Publish(new EnsureInitialisedNotification(), cancellationToken);

        return new Statistic
        {
            PenaltyCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.PenaltyCount),
            ServerCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.ServerCount),
            CommunityCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.CommunityCount),
            PlayerCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.PlayerCount),
        };
    }

    public Task<int> Handle(GetOnlinePlayersCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(statisticsCache.OnlinePlayers.Count);
    }

    public Task<int> Handle(GetRecentBansCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(statisticsCache.RecentBans.Count);
    }

    public Task<Dictionary<string, int>> Handle(GetServerOnlineCountsCommand request, CancellationToken cancellationToken)
    {
        var serverOnlineCount = new Dictionary<string, int>(/*_statisticsCache.ServerOnlineCount*/); // TODO: Implement this.
        return Task.FromResult(serverOnlineCount);
    }
}
