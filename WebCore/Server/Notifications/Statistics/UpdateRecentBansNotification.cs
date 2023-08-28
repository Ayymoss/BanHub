using BanHub.WebCore.Server.Models;
using MediatR;

namespace BanHub.WebCore.Server.Notifications.Statistics;

public class UpdateRecentBansNotification : INotification
{
    public Guid BanGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
