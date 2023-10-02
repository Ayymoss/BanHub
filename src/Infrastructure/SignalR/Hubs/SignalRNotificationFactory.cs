using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.Infrastructure.SignalR.Hubs;

public class SignalRNotificationFactory(IHubContext<StatisticsHub> statisticsHubContext, IHubContext<PluginHub> pluginHubContext,
    IHubContext<TomatoCounterHub> tomatoHubContext, IHubContext<ActiveUserHub> activeUserHub) : ISignalRNotification
{
    public Task NotifyUserAsync(HubType hubType, string methodName, object message, CancellationToken cancellationToken)
    {
        return hubType switch
        {
            HubType.Statistics => statisticsHubContext.Clients.All.SendAsync(methodName, message, cancellationToken),
            HubType.Plugin => pluginHubContext.Clients.All.SendAsync(methodName, message, cancellationToken),
            HubType.TomatoCounter => tomatoHubContext.Clients.All.SendAsync(methodName, message, cancellationToken),
            HubType.ActiveUsers => activeUserHub.Clients.All.SendAsync(methodName, message, cancellationToken),
            _ => throw new ArgumentOutOfRangeException($"Hub type {hubType} not supported.")
        };
    }
}
