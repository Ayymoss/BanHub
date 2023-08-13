using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.Utilities;
using BanHubData.Commands.Community;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Community;

public class CreateOrUpdateCommunityHandler : IRequestHandler<CreateOrUpdateCommunityCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;
    private readonly ApiKeyCache _apiKeyCache;
    private readonly Configuration _config;

    public CreateOrUpdateCommunityHandler(DataContext context, IStatisticService statisticService, ApiKeyCache apiKeyCache,
        Configuration config)
    {
        _context = context;
        _statisticService = statisticService;
        _apiKeyCache = apiKeyCache;
        _config = config;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateCommunityCommand request, CancellationToken cancellationToken)
    {
        if (request.PluginVersion < _config.PluginVersion) return ControllerEnums.ReturnState.BadRequest;

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

            await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.CommunityCount,
                ControllerEnums.StatisticTypeAction.Add);
            await _context.SaveChangesAsync(cancellationToken);

            return ControllerEnums.ReturnState.Created;
        }

        if (community is null || instanceApiValid is null) return ControllerEnums.ReturnState.BadRequest;
        if (community.Id != instanceApiValid.Id) return ControllerEnums.ReturnState.Conflict;

        // Warn if IP address has changed... this really shouldn't happen.
#if !DEBUG
        if (request.HeaderIp != community.CommunityIp)
        {
            community.Active = false;
            _apiKeyCache.TryRemove(request.CommunityGuid);

            IDiscordWebhookSubscriptions.InvokeEvent(new CreateIssueEvent
            {
                CommunityGuid = community.CommunityGuid,
                CommunityIp = request.CommunityIp,
                IncomingIp = request.HeaderIp ?? "Unknown"
            }, cancellationToken);
            return ControllerEnums.ReturnState.BadRequest;
        }
#endif

        // Update existing record
        community.About ??= request.About;
        community.Socials ??= request.Socials;
        community.HeartBeat = DateTimeOffset.UtcNow;
        community.CommunityName = request.CommunityName;
        community.CommunityIpFriendly = request.CommunityWebsite.GetDomainName();
        community.CommunityPort = request.CommunityPort;
        _context.Communities.Update(community);
        await _context.SaveChangesAsync(cancellationToken);

        return ControllerEnums.ReturnState.Ok;
    }
}
