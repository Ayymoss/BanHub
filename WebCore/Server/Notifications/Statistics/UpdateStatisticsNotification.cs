using BanHubData.Enums;
using MediatR;

namespace BanHub.WebCore.Server.Notifications.Statistics;

public class UpdateStatisticsNotification : INotification
{
    public ControllerEnums.StatisticType StatisticType { get; set; }
    public ControllerEnums.StatisticTypeAction StatisticTypeAction { get; set; }
}
