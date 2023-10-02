using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class GetLatestBansHandler(IPenaltyRepository penaltyRepository)
    : IRequestHandler<GetLatestBansCommand, IEnumerable<DTOs.WebView.IndexView.Penalty>>
{
    public async Task<IEnumerable<DTOs.WebView.IndexView.Penalty>> Handle(GetLatestBansCommand request, CancellationToken cancellationToken)
    {
        var bans = await penaltyRepository.GetLatestBansAsync(cancellationToken);

        var view = bans.Select(x => new DTOs.WebView.IndexView.Penalty
        {
            Submitted = x.Submitted,
            RecipientIdentity = x.RecipientIdentity,
            RecipientUserName = x.RecipientUserName,
            CommunityGuid = x.CommunityGuid,
            CommunityName = x.CommunityName,
            Reason = request.Privileged && x.Automated
                ? x.Reason
                : x.Automated
                    ? "Automated Detection"
                    : x.Reason,
        });

        return view;
    }
}
