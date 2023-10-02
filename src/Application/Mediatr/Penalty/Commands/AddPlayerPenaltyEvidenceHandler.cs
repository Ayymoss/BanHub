using BanHub.Application.Mediatr.Services.Events.Discord;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class AddPlayerPenaltyEvidenceHandler(IPublisher publisher, IPenaltyRepository penaltyRepository)
    : IRequestHandler<AddPlayerPenaltyEvidenceCommand, ControllerEnums.ReturnState>
{
    public async Task<ControllerEnums.ReturnState> Handle(AddPlayerPenaltyEvidenceCommand request, CancellationToken cancellationToken)
    {
        var penalty = await penaltyRepository.GetPenaltyAsync(request.PenaltyGuid, cancellationToken);

        if (penalty is null) return ControllerEnums.ReturnState.NotFound;
        // Someone has already submitted evidence. Don't overwrite it.
        if (penalty.Evidence is not null) return ControllerEnums.ReturnState.Conflict;

        penalty.Evidence = request.Evidence;
        var message = $"**Penalty**: {request.PenaltyGuid}\n" +
                      $"**Profile:** [{penalty.Recipient.CurrentAlias.Alias.UserName}](https://BanHub.gg/Players/{penalty.Recipient.Identity})\n" +
                      $"**Evidence**: https://youtu.be/{request.Evidence}\n\n" +
                      $"**Submitted By**: [{request.IssuerUsername}](https://BanHub.gg/Players/{request.IssuerIdentity})";

        await publisher.Publish(new CreateAdminActionNotification
        {
            Title = "Evidence Submitted!",
            Message = message
        }, cancellationToken);

        await penaltyRepository.AddOrUpdatePenaltyAsync(penalty, cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
