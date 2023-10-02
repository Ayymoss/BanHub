using BanHub.Domain.Enums;
using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Statistics;

public class UpdateStatisticsNotification : INotification
{
    public ControllerEnums.StatisticType StatisticType { get; set; }
    public ControllerEnums.StatisticTypeAction StatisticTypeAction { get; set; }
}
