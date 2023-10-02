using BanHub.Application.Mediatr.Services.Events.Discord;
using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Application.Utilities;
using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BanHub.Application.Mediatr.Community.Commands;

public class CreateOrUpdateCommunityHandler(IPluginAuthenticationCache pluginAuthenticationCache, IPublisher publisher,
        ILogger logger, ICommunityRepository communityRepository)
    : IRequestHandler<CreateOrUpdateCommunityCommand, ControllerEnums.ReturnState>
{
    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateCommunityCommand request, CancellationToken cancellationToken)
    {
        var community = await communityRepository.GetCommunityAsync(request.CommunityGuid, cancellationToken);

        // New instance
        if (community is null)
        {
            community = CreateCommunity(request);
            await communityRepository.AddOrUpdateCommunityAsync(community, cancellationToken);

            await publisher.Publish(new UpdateStatisticsNotification
            {
                StatisticType = ControllerEnums.StatisticType.CommunityCount,
                StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
            }, cancellationToken);

            logger.LogDebug("Community {CommunityGuid} created", request.CommunityGuid);
            return ControllerEnums.ReturnState.Created;
        }

        var instanceApiValid = await communityRepository
            .IsCommunityApiKeyValidAsync(request.CommunityGuid, request.CommunityApiKey, cancellationToken);

        if (!instanceApiValid)
        {
            logger.LogDebug("Community {RequestCommunityGuid} already exists", request.CommunityGuid);
            return ControllerEnums.ReturnState.Conflict;
        }

        // Warn if IP address has changed... this really shouldn't happen.

        if (!Generic.IsDebug && request.HeaderIp != community.CommunityIp)
        {
            await DeactivateCommunity(request, community, cancellationToken);
            return ControllerEnums.ReturnState.BadRequest;
        }

        // Update existing record
        community.About ??= request.About;
        community.Socials ??= request.Socials;
        community.HeartBeat = DateTimeOffset.UtcNow;
        community.CommunityName = request.CommunityName;
        community.CommunityIpFriendly = request.CommunityWebsite.GetDomainName();
        community.CommunityPort = request.CommunityPort;

        await communityRepository.AddOrUpdateCommunityAsync(community, cancellationToken);

        logger.LogDebug("Community {CommunityGuid} updated", request.CommunityGuid);
        return ControllerEnums.ReturnState.Ok;
    }

    private async Task DeactivateCommunity(CreateOrUpdateCommunityCommand request, EFCommunity community, CancellationToken cancellationToken)
    {
        community.Active = false;
        pluginAuthenticationCache.TryRemove(request.CommunityGuid);

        await publisher.Publish(new CreateIssueNotification
        {
            CommunityGuid = community.CommunityGuid,
            CommunityIp = request.CommunityIp,
            IncomingIp = request.HeaderIp ?? "Unknown"
        }, cancellationToken);

        await communityRepository.AddOrUpdateCommunityAsync(community, cancellationToken);

        logger.LogWarning("Community {CommunityGuid} IP address changed", request.CommunityGuid);
    }

    private static EFCommunity CreateCommunity(CreateOrUpdateCommunityCommand request)
    {
        return new EFCommunity
        {
            CommunityGuid = request.CommunityGuid,
            CommunityName = request.CommunityName,
            CommunityIp = request.CommunityIp,
            CommunityIpFriendly = request.CommunityWebsite.GetDomainName(),
            CommunityPort = request.CommunityPort,
            ApiKey = request.CommunityApiKey,
            Active = false,
            HeartBeat = DateTimeOffset.UtcNow,
            Created = DateTimeOffset.UtcNow,
            About = request.About,
            Socials = request.Socials
        };
    }
}
