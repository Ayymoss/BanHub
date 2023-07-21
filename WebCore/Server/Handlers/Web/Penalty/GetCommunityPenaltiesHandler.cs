using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Penalty;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

public class GetCommunityPenaltiesHandler : IRequestHandler<GetCommunityPenaltiesCommand,
    IEnumerable<Shared.Models.CommunityProfileView.Penalty>>
{
    private readonly DataContext _context;

    public GetCommunityPenaltiesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.CommunityProfileView.Penalty>> Handle(GetCommunityPenaltiesCommand request,
        CancellationToken cancellationToken)
    {
        var results = await _context.Penalties
            .Where(x => x.Community.CommunityGuid == request.Identity)
            .Select(x => new Shared.Models.CommunityProfileView.Penalty
            {
                PenaltyGuid = x.PenaltyGuid,
                TargetIdentity = x.Recipient.Identity,
                AdminIdentity = x.Issuer.Identity,
                AdminUserName = x.Issuer.CurrentAlias.Alias.UserName,
                Reason = request.Privileged && x.Automated
                    ? x.Reason
                    : x.Automated
                        ? "Automated Detection"
                        : x.Reason,
                Evidence = x.Evidence,
                TargetUserName = x.Recipient.CurrentAlias.Alias.UserName,
                CommunityGuid = x.Community.CommunityGuid,
                Expiration = x.Expiration,
                PenaltyType = x.PenaltyType,
                PenaltyScope = x.PenaltyScope,
                PenaltyStatus = x.PenaltyStatus,
                Submitted = x.Submitted
            }).ToListAsync(cancellationToken: cancellationToken);
        return results;
    }
}
