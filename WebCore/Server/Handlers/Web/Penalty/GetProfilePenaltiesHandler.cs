using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

public class GetProfilePenaltiesHandler : IRequestHandler<GetProfilePenaltiesCommand, IEnumerable<Shared.Models.PlayerProfileView.Penalty>>
{
    private readonly DataContext _context;

    public GetProfilePenaltiesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.PlayerProfileView.Penalty>> Handle(GetProfilePenaltiesCommand request,
        CancellationToken cancellationToken)
    {
        var results = await _context.Penalties
            .Where(x => x.Target.Identity == request.Identity)
            .Select(x => new Shared.Models.PlayerProfileView.Penalty
            {
                PenaltyGuid = x.PenaltyGuid,
                AdminUserName = x.Admin.CurrentAlias.Alias.UserName,
                AdminIdentity = x.Admin.Identity,
                Reason = request.Privileged && x.Automated
                    ? x.Reason
                    : x.Automated
                        ? "Automated Detection"
                        : x.Reason,
                Evidence = x.Evidence,
                InstanceName = x.Instance.InstanceName,
                InstanceGuid = x.Instance.InstanceGuid,
                Expiration = x.Expiration,
                PenaltyType = x.PenaltyType,
                PenaltyScope = x.PenaltyScope,
                PenaltyStatus = x.PenaltyStatus,
                Submitted = x.Submitted
            }).ToListAsync(cancellationToken: cancellationToken);
        return results;
    }
}
