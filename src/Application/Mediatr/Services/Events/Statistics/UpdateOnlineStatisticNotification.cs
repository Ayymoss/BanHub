using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Statistics;

public class UpdateOnlineStatisticNotification : INotification
{
    public IEnumerable<string> Identities { get; set; }
}
