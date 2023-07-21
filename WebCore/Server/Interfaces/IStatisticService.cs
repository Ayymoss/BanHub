using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatisticAsync(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action);
    Task<Statistic> GetStatisticsAsync();
    Task UpdateOnlineStatisticAsync(IEnumerable<string> playerIdentities);
    Task UpdateRecentBansStatisticAsync(StatisticBan statisticBan);
}
