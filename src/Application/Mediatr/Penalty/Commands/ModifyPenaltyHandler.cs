using BanHub.Application.Mediatr.Services.Events.Discord;
using BanHub.Application.Mediatr.Services.Events.Statistics;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class ModifyPenaltyHandler(IPublisher publisher, IPenaltyRepository penaltyRepository) : IRequestHandler<RemovePenaltyCommand, bool>
{
    public async Task<bool> Handle(RemovePenaltyCommand request, CancellationToken cancellationToken)
    {
        var penalty = await penaltyRepository.GetPenaltyAsync(request.PenaltyGuid, cancellationToken);
        if (penalty is null) return false;

        var message = $"**Penalty**: {penalty.PenaltyGuid}\n" +
                      $"**Issuer**: [{penalty.Issuer.CurrentAlias.Alias.UserName}](https://BanHub.gg/Players/{penalty.Issuer.Identity})\n" +
                      $"**Offender**: [{penalty.Recipient.CurrentAlias.Alias.UserName}](https://BanHub.gg/Players/{penalty.Recipient.Identity})\n" +
                      $"**Reason**: {penalty.Reason}\n" +
                      $"**Evidence**: {penalty.Evidence ?? "None"}\n\n" +
                      $"**Modified By**: [{request.IssuerUserName}](https://BanHub.gg/Players/{request.IssuerIdentity})\n" +
                      $"**Modified For**: {request.ModifyPenalty.ToString()} - {request.DeletionReason}";

        switch (request.ModifyPenalty)
        {
            case ModifyPenalty.Revoke:
                penalty.PenaltyStatus = PenaltyStatus.Revoked;
                await penaltyRepository.AddOrUpdatePenaltyAsync(penalty, cancellationToken);
                break;
            case ModifyPenalty.Global:
                penalty.PenaltyScope = PenaltyScope.Global;
                await penaltyRepository.AddOrUpdatePenaltyAsync(penalty, cancellationToken);
                break;
            case ModifyPenalty.Local:
                penalty.PenaltyScope = PenaltyScope.Local;
                await penaltyRepository.AddOrUpdatePenaltyAsync(penalty, cancellationToken);
                break;
            case ModifyPenalty.Delete:
                await penaltyRepository.RemovePenaltyAsync(penalty, cancellationToken);
                await publisher.Publish(new UpdateStatisticsNotification
                {
                    StatisticType = ControllerEnums.StatisticType.PenaltyCount,
                    StatisticTypeAction = ControllerEnums.StatisticTypeAction.Subtract
                }, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.ModifyPenalty), request.ModifyPenalty,
                    "Invalid penalty modification type");
        }

        await publisher.Publish(new CreateAdminActionNotification
        {
            Title = "Penalty Modified!",
            Message = message
        }, cancellationToken);

        return true;
    }
}
