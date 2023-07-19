using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Instance;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Instance;

public class CreateOrUpdateInstanceHandler : IRequestHandler<CreateOrUpdateInstanceCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public CreateOrUpdateInstanceHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<ControllerEnums.ReturnState> Handle(CreateOrUpdateInstanceCommand request,
        CancellationToken cancellationToken)
    {
        var instanceGuid = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(server => server.InstanceGuid == request.InstanceGuid,
                cancellationToken: cancellationToken);

        var instanceApi = await _context.Instances
            .FirstOrDefaultAsync(server => server.ApiKey == request.InstanceApiKey,
                cancellationToken: cancellationToken);

        var ipAddress = request.InstanceIp;

        // New instance
        if (instanceApi is null && instanceGuid is null)
        {
            _context.Instances.Add(new EFInstance
            {
                InstanceGuid = request.InstanceGuid,
                InstanceName = request.InstanceName,
                InstanceIp = ipAddress,
                InstanceIpFriendly = request.InstanceWebsite,
                InstancePort = request.InstanceBindPort,
                ApiKey = request.InstanceApiKey,
                Active = false,
                HeartBeat = DateTimeOffset.UtcNow,
                Created = DateTimeOffset.UtcNow,
                About = request.About,
                Socials = request.Socials
            });

            await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.InstanceCount,
                ControllerEnums.StatisticTypeAction.Add);
            await _context.SaveChangesAsync(cancellationToken);

            return ControllerEnums.ReturnState.Created;
        }

        if (instanceGuid is null || instanceApi is null) return ControllerEnums.ReturnState.BadRequest;
        if (instanceGuid.Id != instanceApi.Id) return ControllerEnums.ReturnState.Conflict;

        // Warn if IP address has changed... this really shouldn't happen.
        if (request.HeaderIp != instanceGuid.InstanceIp)
        {
            IDiscordWebhookSubscriptions.InvokeEvent(new CreateIssueEvent
            {
                InstanceGuid = instanceGuid.InstanceGuid,
                InstanceIp = request.InstanceIp,
                IncomingIp = request.HeaderIp ?? "Unknown"
            }, cancellationToken);
        }

        // Update existing record
        instanceGuid.About ??= request.About;
        instanceGuid.Socials ??= request.Socials;
        instanceGuid.HeartBeat = DateTimeOffset.UtcNow;
        instanceGuid.InstanceName = request.InstanceName;
        instanceGuid.InstanceIpFriendly = request.InstanceWebsite;
        instanceGuid.InstancePort = request.InstanceBindPort;
        _context.Instances.Update(instanceGuid);
        await _context.SaveChangesAsync(cancellationToken);

        return ControllerEnums.ReturnState.Ok;
    }
}
