using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;
using BanHubData.Enums;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Penalty;

public class AddPlayerPenaltyEvidenceHandler(DataContext context, IPublisher publisher) : IRequestHandler<AddPlayerPenaltyEvidenceCommand, ControllerEnums.ReturnState>
{
    public async Task<ControllerEnums.ReturnState> Handle(AddPlayerPenaltyEvidenceCommand request, CancellationToken cancellationToken)
    {
        var penalty = await context.Penalties
            .Where(x => x.PenaltyGuid == request.PenaltyGuid)
            .Select(x => new
            {
                x.Id,
                x.Recipient.Identity,
                x.Recipient.CurrentAlias.Alias.UserName,
                x.Evidence
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (penalty is null) return ControllerEnums.ReturnState.NotFound;
        // Someone has already submitted evidence. Don't overwrite it.
        if (penalty.Evidence is not null) return ControllerEnums.ReturnState.Conflict;

        var newPenalty = await context.Penalties
            .AsTracking()
            .FirstAsync(x => x.Id == penalty.Id, cancellationToken: cancellationToken);

        newPenalty.Evidence = request.Evidence;

        var message = $"**Penalty**: {request.PenaltyGuid}\n" +
                      $"**Profile:** [{penalty.UserName}](https://BanHub.gg/Players/{penalty.Identity})\n" +
                      $"**Evidence**: https://youtu.be/{request.Evidence}\n\n" +
                      $"**Submitted By**: [{request.IssuerUsername}](https://BanHub.gg/Players/{request.IssuerIdentity})";

        await publisher.Publish(new CreateAdminActionNotification
        {
            Title = "Evidence Submitted!",
            Message = message
        }, cancellationToken);

        context.Penalties.Update(newPenalty);
        await context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
