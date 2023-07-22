using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Instance;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Community;

public class CreateOrUpdateCommunityHandler : IRequestHandler<CreateOrUpdateCommunityCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public CreateOrUpdateCommunityHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateCommunityCommand request,
        CancellationToken cancellationToken)
    {
        var instanceGuid = await _context.Communities
            .AsTracking()
            .FirstOrDefaultAsync(server => server.CommunityGuid == request.CommunityGuid,
                cancellationToken: cancellationToken);

        var instanceApi = await _context.Communities
            .FirstOrDefaultAsync(server => server.ApiKey == request.CommunityApiKey,
                cancellationToken: cancellationToken);

        var ipAddress = request.CommunityIp;

        // New instance
        if (instanceApi is null && instanceGuid is null)
        {
            _context.Communities.Add(new EFCommunity
            {
                CommunityGuid = request.CommunityGuid,
                CommunityName = request.CommunityName,
                CommunityIp = ipAddress,
                CommunityIpFriendly = request.CommunityWebsite,
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

        if (instanceGuid is null || instanceApi is null) return ControllerEnums.ReturnState.BadRequest;
        if (instanceGuid.Id != instanceApi.Id) return ControllerEnums.ReturnState.Conflict;

        // Warn if IP address has changed... this really shouldn't happen.
        if (request.HeaderIp != instanceGuid.CommunityIp)
        {
            IDiscordWebhookSubscriptions.InvokeEvent(new CreateIssueEvent
            {
                CommunityGuid = instanceGuid.CommunityGuid,
                CommunityIp = request.CommunityIp,
                IncomingIp = request.HeaderIp ?? "Unknown"
            }, cancellationToken);
        }

        // Update existing record
        instanceGuid.About ??= request.About;
        instanceGuid.Socials ??= request.Socials;
        instanceGuid.HeartBeat = DateTimeOffset.UtcNow;
        instanceGuid.CommunityName = request.CommunityName;
        instanceGuid.CommunityIpFriendly = request.CommunityWebsite;
        instanceGuid.CommunityPort = request.CommunityPort;
        _context.Communities.Update(instanceGuid);
        await _context.SaveChangesAsync(cancellationToken);

        return ControllerEnums.ReturnState.Ok;
    }
}
