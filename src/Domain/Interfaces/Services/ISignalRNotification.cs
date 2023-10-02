using BanHub.Domain.Enums;

namespace BanHub.Domain.Interfaces.Services;

public interface ISignalRNotification
{
    Task NotifyUserAsync(HubType hubType, string methodName, object message, CancellationToken cancellationToken);
}
