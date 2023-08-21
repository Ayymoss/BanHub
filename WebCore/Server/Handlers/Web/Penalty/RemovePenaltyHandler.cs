using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

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
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                IssuerIdentity = x.Issuer.Identity,
                RecipientUserName = x.Recipient.CurrentAlias.Alias.UserName,
                RecipientIdentity = x.Recipient.Identity,
                x.PenaltyGuid,
                x.Reason,
                x.Evidence
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var message = penaltyInfo is null
            ? $"Penalty **{penalty.PenaltyGuid}** was deleted by **{request.IssuerUserName}** but no information could be found."
            : $"**Penalty**: {penaltyInfo.PenaltyGuid}\n" +
              $"**Issuer**: [{penaltyInfo.IssuerUserName}](https://BanHub.gg/Players/{penaltyInfo.RecipientIdentity})\n" +
              $"**Admin**: {penaltyInfo.IssuerIdentity}\n" +
              $"**Offender**: [{penaltyInfo.RecipientUserName}](https://BanHub.gg/Players/{penaltyInfo.RecipientIdentity})\n" +
              $"**Target**: {penaltyInfo.RecipientIdentity}\n" +
              $"**Reason**: {penaltyInfo.Reason}\n" +
              $"**Evidence**: {penaltyInfo.Evidence ?? "None"}\n\n" +
              $"**Deleted By**: [{request.IssuerUserName}](https://BanHub.gg/Players/{request.IssuerIdentity})\n" +
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
