using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.Interfaces.Services;
using BanHub.Domain.ValueObjects.Plugin.SignalR;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class ToggleCommunityActivationHandler(IPluginAuthenticationCache pluginAuthenticationCache,
        ICommunityRepository communityRepository, ISignalRNotification signalRNotification, ICommunityConnectionManager connectionManager)
    : IRequestHandler<ToggleCommunityActivationCommand, bool>
{
    public async Task<bool> Handle(ToggleCommunityActivationCommand request, CancellationToken cancellationToken)
    {
        var community = await communityRepository.GetCommunityAsync(request.CommunityGuid, cancellationToken);
        if (community is null || pluginAuthenticationCache.IsEmpty) return false;

        community.Active = !community.Active;
        if (community.Active) pluginAuthenticationCache.TryAdd(community.CommunityGuid, community.ApiKey);
        else pluginAuthenticationCache.TryRemove(community.CommunityGuid);

        if (connectionManager.TryGetConnectionId(community.CommunityGuid, out var connectionId) && !string.IsNullOrEmpty(connectionId))
            await TellCommunityChange(community.ApiKey, community.Active, cancellationToken);

        await communityRepository.AddOrUpdateCommunityAsync(community, cancellationToken);
        return true;
    }

    private async Task TellCommunityChange(Guid apiKey, bool activated, CancellationToken cancellationToken)
    {
        await signalRNotification.NotifyUserAsync(HubType.Plugin, SignalRMethods.PluginMethods.ActivateCommunity, new ActivateCommunity
        {
            Activated = activated,
            ApiKey = apiKey
        }, cancellationToken);
    }
}
