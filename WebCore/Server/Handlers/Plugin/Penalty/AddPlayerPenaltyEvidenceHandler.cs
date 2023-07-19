using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHubData.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Penalty;

public class AddPlayerPenaltyEvidenceHandler : IRequestHandler<AddPlayerPenaltyEvidenceCommand, ControllerEnums.ReturnState>
{
    private readonly DataContext _context;

    public AddPlayerPenaltyEvidenceHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<ControllerEnums.ReturnState> Handle(AddPlayerPenaltyEvidenceCommand request, CancellationToken cancellationToken)
    {
        var penalty = await _context.Penalties
            .AsTracking()
            .FirstOrDefaultAsync(x => x.PenaltyGuid == request.PenaltyGuid, cancellationToken: cancellationToken);

        if (penalty is null) return ControllerEnums.ReturnState.NotFound;
        // Someone has already submitted evidence. Don't overwrite it.
        if (penalty.Evidence is not null) return ControllerEnums.ReturnState.Conflict;

        penalty.Evidence = request.Evidence;
        var message = $"**Penalty**: {penalty.PenaltyGuid}\n" +
                      $"**Profile:** [{request.OffenderUsername}](https://BanHub.gg/Players/{request.OffenderIdentity})" +
                      $"**Evidence**: https://youtu.be/{penalty.Evidence}\n\n" +
                      $"**Submitted By**: [{request.IssuerUsername}](https://BanHub.gg/Players/{request.IssuerIdentity})";

        IDiscordWebhookSubscriptions.InvokeEvent(new CreateAdminActionEvent
        {
            Title = "Evidence Submitted!",
            Message = message
        }, cancellationToken);

        _context.Penalties.Update(penalty);
        await _context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
