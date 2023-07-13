using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Penalty;

public class GetLatestBansHandler : IRequestHandler<GetLatestBansCommand, IEnumerable<Shared.Models.IndexView.Penalty>>
{
    private readonly DataContext _context;

    public GetLatestBansHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.IndexView.Penalty>> Handle(GetLatestBansCommand request, CancellationToken cancellationToken)
    {
        var bans = await _context.Penalties
            .Where(x => x.PenaltyType == PenaltyType.Ban)
            .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
            .Where(x => x.PenaltyScope == PenaltyScope.Global)
            .Where(x => x.Submitted > DateTimeOffset.UtcNow.AddMonths(-1)) // Arbitrary time frame. We don't care about anything too old.
            .OrderByDescending(x => x.Id)
            .Take(3)
            .Select(penalty => new Shared.Models.IndexView.Penalty
            {
                Submitted = penalty.Submitted,
                TargetIdentity = penalty.Target.Identity,
                TargetUserName = penalty.Target.CurrentAlias.Alias.UserName,
                InstanceGuid = penalty.Instance.InstanceGuid,
                InstanceName = penalty.Instance.InstanceName,
                Reason = penalty.Reason
            }).ToListAsync(cancellationToken: cancellationToken);

        return bans;
    }
}
