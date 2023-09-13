using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;

public class UpdateOnlineStatisticNotification : INotification
{
    public IEnumerable<string> Identities { get; set; }
}
