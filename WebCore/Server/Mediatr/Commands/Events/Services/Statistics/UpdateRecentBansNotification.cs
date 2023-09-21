using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;

public class UpdateRecentBansNotification : INotification
{
    public Guid BanGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
