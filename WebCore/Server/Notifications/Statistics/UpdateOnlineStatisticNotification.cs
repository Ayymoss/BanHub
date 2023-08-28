using MediatR;

namespace BanHub.WebCore.Server.Notifications.Statistics;

public class UpdateOnlineStatisticNotification : INotification
{
    public IEnumerable<string> Identities { get; set; }
}
