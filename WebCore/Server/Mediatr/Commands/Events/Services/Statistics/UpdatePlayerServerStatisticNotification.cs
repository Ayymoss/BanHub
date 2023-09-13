using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;

public class UpdatePlayerServerStatisticNotification : INotification
{
    public string Identity { get; set; }
    public string ServerId { get; set; }
}
