﻿using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.ViewModels;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatisticAsync(ControllerEnums.StatisticType statistic, ControllerEnums.StatisticTypeAction action);
    Task<Statistic> GetStatisticsAsync();
    Task UpdateOnlineStatisticAsync(StatisticUsersOnline statisticUsers);
    Task UpdateDayStatisticAsync(StatisticBan statisticBan);
}
