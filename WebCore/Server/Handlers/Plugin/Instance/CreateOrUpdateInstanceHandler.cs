using BanHub.WebCore.Server.Context;
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
            .FirstOrDefaultAsync(server => server.ApiKey == request.InstanceApiKey, cancellationToken: cancellationToken);

        var ipAddress = request.InstanceIp;

        // New instance
        if (instanceApi is null && instanceGuid is null)
        {
            _context.Instances.Add(new EFInstance
            {
                InstanceGuid = request.InstanceGuid,
                InstanceIp = ipAddress,
                InstanceName = request.InstanceName,
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

        // TODO: Update this... It doesn't check a mismatch...
        if (instanceGuid is null || instanceApi is null) return ControllerEnums.ReturnState.BadRequest;
        if (instanceGuid.Id != instanceApi.Id) return ControllerEnums.ReturnState.Conflict;

        // Warn if IP address has changed... this really shouldn't happen.
        //if (requestIpAddress is not null && requestIpAddress != instanceGuid.InstanceIp)
        //{
        //    await _discordWebhook.CreateIssueHookAsync(instanceGuid.InstanceGuid, request.InstanceIp!,
        //        requestIpAddress);
        //} // TODO: Reimplement this. We need to know if someone else has stolen the API key.

        // Update existing record
        instanceGuid.About ??= request.About;
        instanceGuid.Socials ??= request.Socials;
        instanceGuid.HeartBeat = DateTimeOffset.UtcNow;
        instanceGuid.InstanceName = request.InstanceName;
        _context.Instances.Update(instanceGuid);
        await _context.SaveChangesAsync(cancellationToken);

        return instanceGuid.Active
            ? ControllerEnums.ReturnState.Accepted // Active
            : ControllerEnums.ReturnState.Ok; // Inactive
    }
}
