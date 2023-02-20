using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatisticAsync(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action);
    Task<StatisticDto> GetStatisticsAsync();
    Task UpdateOnlineStatisticAsync(StatisticUsersOnline statisticUsers);
    Task UpdateDayStatisticAsync(StatisticBan statisticBan);
}
