using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Statistics;
using BanHub.WebCore.Server.Domains;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Utilities;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Community;

public class CreateOrUpdateCommunityHandler : IRequestHandler<CreateOrUpdateCommunityCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IPluginAuthenticationCache _pluginAuthenticationCache;
    private readonly IMediator _mediator;
    private readonly Configuration _config;
    private readonly ILogger<CreateOrUpdateCommunityHandler> _logger;

    public CreateOrUpdateCommunityHandler(DataContext context, IPluginAuthenticationCache pluginAuthenticationCache, IMediator mediator,
        Configuration config, ILogger<CreateOrUpdateCommunityHandler> logger)
    {
        _context = context;
        _pluginAuthenticationCache = pluginAuthenticationCache;
        _mediator = mediator;
        _config = config;
        _logger = logger;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateCommunityCommand request, CancellationToken cancellationToken)
    {
        var community = await _context.Communities
            .AsTracking()
            .FirstOrDefaultAsync(server => server.CommunityGuid == request.CommunityGuid,
                cancellationToken: cancellationToken);

        var instanceApiValid = await _context.Communities
            .Where(server => server.ApiKey == request.CommunityApiKey)
            .Select(x => new {x.Id})
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        // New instance
        if (instanceApiValid is null && community is null)
        {
            _context.Communities.Add(new EFCommunity
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

            await _mediator.Publish(new UpdateStatisticsNotification
            {
                StatisticType = ControllerEnums.StatisticType.CommunityCount,
                StatisticTypeAction = ControllerEnums.StatisticTypeAction.Add
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Community {CommunityGuid} created", request.CommunityGuid);
            return ControllerEnums.ReturnState.Created;
        }

        if (community is null || instanceApiValid is null)
        {
            _logger.LogDebug("Community {RequestCommunityGuid} not found", request.CommunityGuid);
            return ControllerEnums.ReturnState.BadRequest;
        }

        if (community.Id != instanceApiValid.Id)
        {
            _logger.LogDebug("Community {RequestCommunityGuid} already exists", request.CommunityGuid);
            return ControllerEnums.ReturnState.Conflict;
        }

        // Warn if IP address has changed... this really shouldn't happen.

        if (!Shared.Utilities.Utilities.IsDebug)
            if (request.HeaderIp != community.CommunityIp)
            {
                community.Active = false;
                _pluginAuthenticationCache.TryRemove(request.CommunityGuid);

                await _mediator.Publish(new CreateIssueNotification
                {
                    CommunityGuid = community.CommunityGuid,
                    CommunityIp = request.CommunityIp,
                    IncomingIp = request.HeaderIp ?? "Unknown"
                }, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Community {CommunityGuid} IP address changed", request.CommunityGuid);
                return ControllerEnums.ReturnState.BadRequest;
            }


        // Update existing record
        community.About ??= request.About;
        community.Socials ??= request.Socials;
        community.HeartBeat = DateTimeOffset.UtcNow;
        community.CommunityName = request.CommunityName;
        community.CommunityIpFriendly = request.CommunityWebsite.GetDomainName();
        community.CommunityPort = request.CommunityPort;
        _context.Communities.Update(community);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Community {CommunityGuid} updated", request.CommunityGuid);
        return ControllerEnums.ReturnState.Ok;
    }
}
