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
            .Where(x => x.Recipient.Identity == request.Identity)
            .Select(x => new Shared.Models.PlayerProfileView.Penalty
            {
                PenaltyGuid = x.PenaltyGuid,
                AdminUserName = x.Issuer.CurrentAlias.Alias.UserName,
                AdminIdentity = x.Issuer.Identity,
                Reason = !request.Privileged && x.Automated
                    ? "Automated Detection"
                    : x.Reason,
                Evidence = x.Evidence,
                CommunityName = x.Community.CommunityName,
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
