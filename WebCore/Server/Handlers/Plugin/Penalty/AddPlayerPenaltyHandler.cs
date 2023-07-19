using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Penalty;

public class AddPlayerPenaltyHandler : IRequestHandler<AddPlayerPenaltyCommand, (ControllerEnums.ReturnState, Guid?)>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public AddPlayerPenaltyHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<(ControllerEnums.ReturnState, Guid?)> Handle(AddPlayerPenaltyCommand request, CancellationToken cancellationToken)
    {
        var target = await _context.Players.AsTracking()
            .Where(x => x.Identity == request.TargetIdentity)
            .Select(x => new
            {
                x.Id,
                x.Identity,
                x.CurrentAlias.Alias.UserName,
                x.CurrentAlias.Alias.IpAddress
            }).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        var admin = await _context.Players.AsTracking()
            .Where(x => x.Identity == request.AdminIdentity)
            .Select(x => new
            {
                x.Id
            }).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        var instance = await _context.Instances.AsTracking()
            .Where(x => x.InstanceGuid == request.InstanceGuid)
            .Select(x => new
            {
                x.Id,
                x.InstanceName,
                x.InstanceGuid
            }).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (target is null || admin is null || instance is null) return (ControllerEnums.ReturnState.BadRequest, null);

        var penalties = await _context.Penalties
            .AsTracking()
            .Include(x => x.Identifier)
            .Where(i => i.Instance.InstanceGuid == request.InstanceGuid)
            .Where(t => t.TargetId == target.Id)
            .Where(p => p.PenaltyType == PenaltyType.Ban || p.PenaltyType == PenaltyType.TempBan)
            .Where(p => p.PenaltyStatus == PenaltyStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);

        var hasExistingGlobalBan = await _context.Penalties
            .Where(x => x.TargetId == target.Id)
            .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
            .Where(x => x.PenaltyType == PenaltyType.Ban)
            .AnyAsync(x => x.PenaltyScope == PenaltyScope.Global, cancellationToken: cancellationToken);

        switch (request.PenaltyType)
        {
            // Revoke existing bans from the same instance
            case PenaltyType.Unban when penalties.Any():
            {
                foreach (var inf in penalties)
                {
                    inf.PenaltyStatus = PenaltyStatus.Revoked;
                    _context.Penalties.Update(inf);
                    if (inf.Identifier is not null) _context.PenaltyIdentifiers.Remove(inf.Identifier);
                }

                break;
            }
            // If they're kicked for an already existing ban, ignore.
            case PenaltyType.Kick when penalties.Any():
                return (ControllerEnums.ReturnState.Conflict, null);
            // Unban when they have no penalties?! Back out.
            case PenaltyType.Unban when !penalties.Any():
                return (ControllerEnums.ReturnState.NotFound, null);
            // Back out if they already have a global ban
            case PenaltyType.Ban when hasExistingGlobalBan && request.PenaltyScope is PenaltyScope.Global:
                return (ControllerEnums.ReturnState.Conflict, null);
        }

        var penaltyModel = new EFPenalty
        {
            PenaltyType = request.PenaltyType,
            PenaltyStatus = request.PenaltyType is PenaltyType.Ban or PenaltyType.TempBan
                ? PenaltyStatus.Active
                : PenaltyStatus.Informational,
            PenaltyScope = request.PenaltyScope,
            PenaltyGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            AdminId = admin.Id,
            Reason = request.Reason,
            Automated = request.Automated,
            Expiration = request.Expiration,
            InstanceId = instance.Id,
            TargetId = target.Id
        };

        if (request is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global})
        {
            var identifier = new EFPenaltyIdentifier
            {
                Identity = target.Identity,
                IpAddress = target.IpAddress,
                Expiration = DateTimeOffset.UtcNow.AddMonths(1),
                Penalty = penaltyModel,
                PlayerId = target.Id
            };
            _context.Add(identifier);
        }

        if (request.PenaltyScope is PenaltyScope.Global)
            await _statisticService.UpdateDayStatisticAsync(new StatisticBan
            {
                BanGuid = penaltyModel.PenaltyGuid,
                Submitted = DateTimeOffset.UtcNow
            });

        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.PenaltyCount,
            ControllerEnums.StatisticTypeAction.Add);
        _context.Add(penaltyModel);
        await _context.SaveChangesAsync(cancellationToken);

        if (request is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global})
            IDiscordWebhookSubscriptions.InvokeEvent(new CreatePenaltyEvent
            {
                Scope = penaltyModel.PenaltyScope,
                PenaltyType = penaltyModel.PenaltyType,
                PenaltyGuid = penaltyModel.PenaltyGuid,
                TargetIdentity = target.Identity,
                Username = target.UserName,
                Reason = penaltyModel.Reason,
                InstanceGuid = instance.InstanceGuid,
                InstanceName = instance.InstanceName
            }, cancellationToken);

        return (ControllerEnums.ReturnState.Created, penaltyModel.PenaltyGuid);
    }
}
