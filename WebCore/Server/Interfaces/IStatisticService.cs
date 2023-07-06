using Data.Enums;
using BanHub.WebCore.Server.Models;
using Data.Domains;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatisticAsync(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action);
    Task<Statistic> GetStatisticsAsync();
    Task UpdateOnlineStatisticAsync(StatisticUsersOnline statisticUsers);
    Task UpdateDayStatisticAsync(StatisticBan statisticBan);
}
