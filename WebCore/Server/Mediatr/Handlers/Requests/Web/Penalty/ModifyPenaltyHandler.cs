using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Penalty;

public class ModifyPenaltyHandler(DataContext context, IPublisher publisher) : IRequestHandler<RemovePenaltyCommand, bool>
{
    public async Task<bool> Handle(RemovePenaltyCommand request, CancellationToken cancellationToken)
    {
        var penalty = await context.Penalties.FirstOrDefaultAsync(x => x.PenaltyGuid == request.PenaltyGuid,
            cancellationToken: cancellationToken);
        if (penalty is null) return false;

        var penaltyInfo = await context.Penalties
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
            ? $"Penalty **{penalty.PenaltyGuid}** was modified by **{request.IssuerUserName}** but no information could be found."
            : $"**Penalty**: {penaltyInfo.PenaltyGuid}\n" +
              $"**Issuer**: [{penaltyInfo.IssuerUserName}](https://BanHub.gg/Players/{penaltyInfo.RecipientIdentity})\n" +
              $"**Offender**: [{penaltyInfo.RecipientUserName}](https://BanHub.gg/Players/{penaltyInfo.RecipientIdentity})\n" +
              $"**Reason**: {penaltyInfo.Reason}\n" +
              $"**Evidence**: {penaltyInfo.Evidence ?? "None"}\n\n" +
              $"**Modified By**: [{request.IssuerUserName}](https://BanHub.gg/Players/{request.IssuerIdentity})\n" +
              $"**Modified For**: {request.ModifyPenalty.ToString()} - {request.DeletionReason}";

        switch (request.ModifyPenalty)
        {
            case ModifyPenalty.Revoke:
                penalty.PenaltyStatus = PenaltyStatus.Revoked;
                break;
            case ModifyPenalty.Global:
                penalty.PenaltyScope = PenaltyScope.Global;
                break;
            case ModifyPenalty.Local:
                penalty.PenaltyScope = PenaltyScope.Local;
                break;
            case ModifyPenalty.Delete:
                context.Penalties.Remove(penalty);
                break;
        }

        await context.SaveChangesAsync(cancellationToken);
        await publisher.Publish(new UpdateStatisticsNotification
        {
            StatisticType = ControllerEnums.StatisticType.PenaltyCount,
            StatisticTypeAction = ControllerEnums.StatisticTypeAction.Remove
        }, cancellationToken);
        await publisher.Publish(new CreateAdminActionNotification
        {
            Title = "Penalty Modified!",
            Message = message
        }, cancellationToken);

        return true;
    }
}
