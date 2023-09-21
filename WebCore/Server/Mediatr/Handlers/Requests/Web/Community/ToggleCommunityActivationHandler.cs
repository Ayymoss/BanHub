using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.SignalR;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;
using BanHubData.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Community;

public class ToggleCommunityActivationHandler(IPluginAuthenticationCache pluginAuthenticationCache, DataContext context,
        IHubContext<PluginHub> pluginHub, ICommunityConnectionManager connectionManager)
    : IRequestHandler<ToggleCommunityActivationCommand, bool>
{
    public async Task<bool> Handle(ToggleCommunityActivationCommand request, CancellationToken cancellationToken)
    {
        var instance = await context.Communities
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);

        if (instance is null || pluginAuthenticationCache.IsEmpty) return false;
        instance.Active = !instance.Active;

        if (instance.Active) pluginAuthenticationCache.TryAdd(instance.CommunityGuid, instance.ApiKey);
        else pluginAuthenticationCache.TryRemove(instance.CommunityGuid);

        await TellCommunityChange(instance.CommunityGuid, instance.ApiKey, instance.Active);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task TellCommunityChange(Guid guid, Guid apiKey, bool activated)
    {
        if (connectionManager.TryGetConnectionId(guid, out var connectionId) && !string.IsNullOrEmpty(connectionId))
        {
            await pluginHub.Clients.Client(connectionId).SendAsync(SignalRMethods.PluginMethods.ActivateCommunity, new ActivateCommunity
            {
                Activated = activated,
                ApiKey = apiKey
            });
        }
    }
}
