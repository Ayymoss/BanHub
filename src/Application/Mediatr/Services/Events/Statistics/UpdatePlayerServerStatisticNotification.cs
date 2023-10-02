using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Statistics;

public class UpdatePlayerServerStatisticNotification : INotification
{
    public string Identity { get; set; }
    public string ServerId { get; set; }
}
