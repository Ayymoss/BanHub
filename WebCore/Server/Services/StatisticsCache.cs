using System.Collections.Concurrent;
using BanHub.WebCore.Server.Interfaces;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Services;

public class StatisticsCache : IStatisticsCache
{
    public ConcurrentDictionary<Guid, DateTimeOffset> RecentBans { get; set; } = new();
    public ConcurrentDictionary<string, DateTimeOffset> OnlinePlayers { get; set; } = new();
    public ConcurrentDictionary<string, ServerOnlineStatistic> OnlineServers { get; set; } = new();
    public bool Loaded { get; set; }
    private int _penalties;
    private int _servers;
    private int _communities;
    private int _players;

    public void UpdateStatisticCount(ControllerEnums.StatisticType type, ControllerEnums.StatisticTypeAction action)
    {
        var statisticMapping = new Dictionary<ControllerEnums.StatisticType, Action>
        {
            {
                ControllerEnums.StatisticType.PenaltyCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _penalties);
                    else Interlocked.Decrement(ref _penalties);
                }
            },
            {
                ControllerEnums.StatisticType.ServerCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _servers);
                    else Interlocked.Decrement(ref _servers);
                }
            },
            {
                ControllerEnums.StatisticType.CommunityCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _communities);
                    else Interlocked.Decrement(ref _communities);
                }
            },
            {
                ControllerEnums.StatisticType.PlayerCount, () =>
                {
                    if (action is ControllerEnums.StatisticTypeAction.Add)
                        Interlocked.Increment(ref _players);
                    else Interlocked.Decrement(ref _players);
                }
            }
        };

        statisticMapping[type]();
    }

    public int GetStatisticCount(ControllerEnums.StatisticType type)
    {
        return type switch
        {
            ControllerEnums.StatisticType.PenaltyCount => _penalties,
            ControllerEnums.StatisticType.ServerCount => _servers,
            ControllerEnums.StatisticType.CommunityCount => _communities,
            ControllerEnums.StatisticType.PlayerCount => _players,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void SetStatisticsCount(ControllerEnums.StatisticType type, int count)
    {
        switch (type)
        {
            case ControllerEnums.StatisticType.PenaltyCount:
                _penalties = count;
                break;
            case ControllerEnums.StatisticType.ServerCount:
                _servers = count;
                break;
            case ControllerEnums.StatisticType.CommunityCount:
                _communities = count;
                break;
            case ControllerEnums.StatisticType.PlayerCount:
                _players = count;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}

public class ServerOnlineStatistic
{
    public int OnlineCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
