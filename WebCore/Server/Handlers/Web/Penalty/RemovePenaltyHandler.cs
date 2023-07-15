using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.PlayerProfile;

public class RemovePenaltyHandler : IRequestHandler<RemovePenaltyCommand, bool>
{
    private readonly DataContext _context;
    private readonly IStatisticService _statisticService;

    public RemovePenaltyHandler(DataContext context, IStatisticService statisticService)
    {
        _context = context;
        _statisticService = statisticService;
    }

    public async Task<bool> Handle(RemovePenaltyCommand request, CancellationToken cancellationToken)
    {
        var penalty = await _context.Penalties.FirstOrDefaultAsync(x => x.PenaltyGuid == request.PenaltyGuid,
            cancellationToken: cancellationToken);
        if (penalty is null) return false;

        var penaltyInfo = await _context.Penalties
            .Where(x => x.PenaltyGuid == penalty.PenaltyGuid)
            .Select(x => new
            {
                AdminIdentity = x.Admin.Identity,
                TargetIdentity = x.Target.Identity,
                x.PenaltyGuid,
                x.Reason,
                x.Evidence
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var message = penaltyInfo is null
            ? $"Penalty **{penalty.PenaltyGuid}** was deleted by **{request.AdminUserName}** but no information could be found."
            : $"**Penalty**: {penaltyInfo.PenaltyGuid}\n" +
              $"**Admin**: {penaltyInfo.AdminIdentity}\n" +
              $"**Target**: {penaltyInfo.TargetIdentity}\n" +
              $"**Reason**: {penaltyInfo.Reason}\n" +
              $"**Evidence**: {penaltyInfo.Evidence ?? "None"}\n\n" +
              $"**Deleted By**: {request.AdminUserName}\n" +
              $"**Deleted For**: {request.DeletionReason}";

        _context.Penalties.Remove(penalty);
        await _context.SaveChangesAsync(cancellationToken);
        await _statisticService.UpdateStatisticAsync(ControllerEnums.StatisticType.PenaltyCount, ControllerEnums.StatisticTypeAction.Remove);
        IDiscordWebhookSubscriptions.InvokeEvent(new CreateAdminActionEvent
        {
            Title = "Penalty Deletion!",
            Message = message
        }, cancellationToken);
        return true;
    }
}
