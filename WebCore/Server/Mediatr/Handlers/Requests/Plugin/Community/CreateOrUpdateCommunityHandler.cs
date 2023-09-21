using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;
using BanHub.WebCore.Server.Domains;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Utilities;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Community;

public class CreateOrUpdateCommunityHandler(DataContext context, IPluginAuthenticationCache pluginAuthenticationCache, IPublisher publisher,
        ILogger logger)
    : IRequestHandler<CreateOrUpdateCommunityCommand, ControllerEnums.ReturnState>
{
    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateCommunityCommand request, CancellationToken cancellationToken)
    {
        var community = await context.Communities
            .AsTracking()
            .FirstOrDefaultAsync(server => server.CommunityGuid == request.CommunityGuid,
                cancellationToken: cancellationToken);

        var instanceApiValid = await context.Communities
            .Where(server => server.ApiKey == request.CommunityApiKey)
            .Select(x => new {x.Id})
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // New instance
        if (instanceApiValid is null && community is null)
        {
            context.Communities.Add(new EFCommunity
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
            });

            await publisher.Publish(new UpdateStatisticsNotification
            {
                StatisticType = ControllerEnums.StatisticType.CommunityCount,
                StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
            }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogDebug("Community {CommunityGuid} created", request.CommunityGuid);
            return ControllerEnums.ReturnState.Created;
        }

        if (community is null || instanceApiValid is null)
        {
            logger.LogDebug("Community {RequestCommunityGuid} not found", request.CommunityGuid);
            return ControllerEnums.ReturnState.BadRequest;
        }

        if (community.Id != instanceApiValid.Id)
        {
            logger.LogDebug("Community {RequestCommunityGuid} already exists", request.CommunityGuid);
            return ControllerEnums.ReturnState.Conflict;
        }

        // Warn if IP address has changed... this really shouldn't happen.

        if (!Shared.Utilities.Utilities.IsDebug)
            if (request.HeaderIp != community.CommunityIp)
            {
                community.Active = false;
                pluginAuthenticationCache.TryRemove(request.CommunityGuid);

                await publisher.Publish(new CreateIssueNotification
                {
                    CommunityGuid = community.CommunityGuid,
                    CommunityIp = request.CommunityIp,
                    IncomingIp = request.HeaderIp ?? "Unknown"
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
                logger.LogWarning("Community {CommunityGuid} IP address changed", request.CommunityGuid);
                return ControllerEnums.ReturnState.BadRequest;
            }


        // Update existing record
        community.About ??= request.About;
        community.Socials ??= request.Socials;
        community.HeartBeat = DateTimeOffset.UtcNow;
        community.CommunityName = request.CommunityName;
        community.CommunityIpFriendly = request.CommunityWebsite.GetDomainName();
        community.CommunityPort = request.CommunityPort;
        context.Communities.Update(community);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogDebug("Community {CommunityGuid} updated", request.CommunityGuid);
        return ControllerEnums.ReturnState.Ok;
    }
}
