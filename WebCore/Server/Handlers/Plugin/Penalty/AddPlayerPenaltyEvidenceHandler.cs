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

        // TODO: This needs to be updated in another way. Maybe remove required tags for efmodel -> using Attach.
        var newPenalty = await _context.Penalties
            .AsTracking()
            .FirstAsync(x => x.Id == penalty.Id, cancellationToken: cancellationToken);
        
        newPenalty.Evidence = request.Evidence;

        var message = $"**Penalty**: {request.PenaltyGuid}\n" +
                      $"**Profile:** [{penalty.UserName}](https://BanHub.gg/Players/{penalty.Identity})\n" +
                      $"**Evidence**: https://youtu.be/{request.Evidence}\n\n" +
                      $"**Submitted By**: [{request.IssuerUsername}](https://BanHub.gg/Players/{request.IssuerIdentity})";

        IDiscordWebhookSubscriptions.InvokeEvent(new CreateAdminActionEvent
        {
            Title = "Evidence Submitted!",
            Message = message
        }, cancellationToken);

        _context.Penalties.Update(newPenalty);
        await _context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
