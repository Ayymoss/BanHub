using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.SignalR;
using BanHub.WebCore.Shared.Mediatr.Commands.Community;
using BanHubData.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Community;

public class ToggleCommunityActivationHandler : IRequestHandler<ToggleCommunityActivationCommand, bool>
{
    private readonly IPluginAuthenticationCache _pluginAuthenticationCache;
    private readonly DataContext _context;
    private readonly IHubContext<PluginHub> _pluginHub;
    private readonly ICommunityConnectionManager _connectionManager;

    public ToggleCommunityActivationHandler(IPluginAuthenticationCache pluginAuthenticationCache, DataContext context, IHubContext<PluginHub> pluginHub,
        ICommunityConnectionManager connectionManager)
    {
        _pluginAuthenticationCache = pluginAuthenticationCache;
        _context = context;
        _pluginHub = pluginHub;
        _connectionManager = connectionManager;
    }

    public async Task<bool> Handle(ToggleCommunityActivationCommand request, CancellationToken cancellationToken)
    {
        var instance = await _context.Communities
            .FirstOrDefaultAsync(x => x.CommunityGuid == request.CommunityGuid, cancellationToken: cancellationToken);

        if (instance is null || _pluginAuthenticationCache.IsEmpty) return false;
        instance.Active = !instance.Active;

        if (instance.Active) _pluginAuthenticationCache.TryAdd(instance.CommunityGuid, instance.ApiKey);
        else _pluginAuthenticationCache.TryRemove(instance.CommunityGuid);

        await TellCommunityChange(instance.CommunityGuid, instance.ApiKey, instance.Active);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task TellCommunityChange(Guid guid, Guid apiKey, bool activated)
    {
        if (_connectionManager.TryGetConnectionId(guid, out var connectionId) && !string.IsNullOrEmpty(connectionId))
        {
            await _pluginHub.Clients.Client(connectionId).SendAsync(SignalRMethods.PluginMethods.ActivateCommunity, new ActivateCommunity
            {
                Activated = activated,
                ApiKey = apiKey
            });
        }
    }
}
