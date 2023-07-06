using BanHub.WebCore.Server.Context;
using Data.Commands.Penalty;
using Data.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Penalty;


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
        _context.Penalties.Update(penalty);
        await _context.SaveChangesAsync(cancellationToken);
        return ControllerEnums.ReturnState.Ok;
    }
}
