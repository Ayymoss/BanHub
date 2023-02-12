using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatistic(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action);
    Task<StatisticDto> GetStatistics();
    Task UpdateOnlineStatistic(StatisticUsersOnline statisticUsers);
    Task UpdateDayStatistic(StatisticBan statisticBan);
}
