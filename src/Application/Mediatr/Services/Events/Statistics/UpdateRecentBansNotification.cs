using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Statistics;

public class UpdateRecentBansNotification : INotification
{
    public Guid BanGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
