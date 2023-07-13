using BanHub.WebCore.Server.Context;
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
    private readonly IDiscordWebhookService _discordWebhook;
    private readonly IStatisticService _statisticService;

    public AddPlayerPenaltyHandler(DataContext context, IDiscordWebhookService discordWebhook,
        IStatisticService statisticService)
    {
        _context = context;
        _discordWebhook = discordWebhook;
        _statisticService = statisticService;
    }

    public async Task<(ControllerEnums.ReturnState, Guid?)> Handle(AddPlayerPenaltyCommand request, CancellationToken cancellationToken)
    {
        var result = await (
            from player in _context.Players.AsTracking().Include(x => x.CurrentAlias.Alias)
            where player.Identity == request.TargetIdentity
            from admin in _context.Players
            where admin.Identity == request.AdminIdentity
            from instance in _context.Instances
            where instance.ApiKey == request.InstanceApiKey
            select new {User = player, Admin = admin, Instance = instance}
        ).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (result?.User is null || result.Admin is null || result.Instance is null)
            return (ControllerEnums.ReturnState.BadRequest, null);

        var penalties = await _context.Penalties
            .AsTracking()
            .Where(i => i.Instance.InstanceGuid == request.InstanceGuid)
            .Where(t => t.TargetId == result.User.Id)
            .Where(p => p.PenaltyType == PenaltyType.Ban || p.PenaltyType == PenaltyType.TempBan)
            .Where(p => p.PenaltyStatus == PenaltyStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);

        switch (request.PenaltyType)
        {
            case PenaltyType.Unban when penalties.Count > 0:
            {
                foreach (var inf in penalties)
                {
                    inf.PenaltyStatus = PenaltyStatus.Revoked;
                    _context.Penalties.Update(inf);
                }

                break;
            }
            case PenaltyType.Kick when penalties.Count > 0:
                return (ControllerEnums.ReturnState.Ok, null);
            case PenaltyType.Unban when penalties.Count == 0:
                return (ControllerEnums.ReturnState.Ok, null);
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
            AdminId = result.Admin.Id,
            Reason = request.Reason,
            Automated = request.Automated,
            Duration = request.Duration,
            InstanceId = result.Instance.Id,
            TargetId = result.User.Id
        };

        if (request is {PenaltyType: PenaltyType.Ban, PenaltyScope: PenaltyScope.Global})
        {
            var identifier = new EFPenaltyIdentifier
            {
                Identity = result.User.Identity,
                IpAddress = result.User.CurrentAlias.Alias.IpAddress,
                Expiration = DateTimeOffset.UtcNow.AddMonths(1),
                Penalty = penaltyModel,
                PlayerId = result.User.Id
            };
            _context.Add(identifier);
        }

        await _statisticService.UpdateDayStatisticAsync(new StatisticBan
        {
            BanGuid = penaltyModel.PenaltyGuid,
            Submitted = DateTimeOffset.UtcNow
        });

        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.PenaltyCount,
            ControllerEnums.StatisticTypeAction.Add);
        _context.Add(penaltyModel);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _discordWebhook.CreatePenaltyHookAsync(penaltyModel.PenaltyScope, penaltyModel.PenaltyType,
                penaltyModel.PenaltyGuid, result.User.Identity, result.User.CurrentAlias.Alias.UserName,
                penaltyModel.Reason);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return (ControllerEnums.ReturnState.Created, penaltyModel.PenaltyGuid);
    }
}
