using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Penalty;

public class GetPenaltiesPaginationHandler : IRequestHandler<GetPenaltiesPaginationCommand,
    PaginationContext<Shared.Models.PenaltiesView.Penalty>>
{
    private readonly DataContext _context;

    public GetPenaltiesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Shared.Models.PenaltiesView.Penalty>> Handle(GetPenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Penalties
            .Where(x => x.PenaltyType == PenaltyType.Ban
                        || x.PenaltyType == PenaltyType.TempBan
                        || x.PenaltyType == PenaltyType.Kick
                        || x.PenaltyType == PenaltyType.Unban)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.PenaltyGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Reason, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Issuer.CurrentAlias.Alias.UserName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Recipient.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "PenaltyGuid" => current.ApplySort(sort, p => p.PenaltyGuid),
                "IssuerUserName" => current.ApplySort(sort, p => p.Issuer.CurrentAlias.Alias.UserName),
                "RecipientUserName" => current.ApplySort(sort, p => p.Recipient.CurrentAlias.Alias.UserName),
                "Reason" => current.ApplySort(sort, p => p.Reason),
                "PenaltyType" => current.ApplySort(sort, p => p.PenaltyType),
                "PenaltyStatus" => current.ApplySort(sort, p => p.PenaltyStatus),
                "PenaltyScope" => current.ApplySort(sort, p => p.PenaltyScope),
                "CommunityName" => current.ApplySort(sort, p => p.Community.CommunityName),
                "Submitted" => current.ApplySort(sort, p => p.Submitted),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(penalty => new Shared.Models.PenaltiesView.Penalty
            {
                PenaltyGuid = penalty.PenaltyGuid,
                PenaltyType = penalty.PenaltyType,
                PenaltyStatus = penalty.PenaltyStatus,
                PenaltyScope = penalty.PenaltyScope,
                Submitted = penalty.Submitted,
                IssuerIdentity = penalty.Issuer.Identity,
                IssuerUserName = penalty.Issuer.CurrentAlias.Alias.UserName,
                Reason = request.Privileged && penalty.Automated
                    ? penalty.Reason
                    : penalty.Automated
                        ? "Automated Detection"
                        : penalty.Reason,
                Evidence = penalty.Evidence,
                Expiration = penalty.Expiration,
                CommunityGuid = penalty.Community.CommunityGuid,
                RecipientIdentity = penalty.Recipient.Identity,
                RecipientUserName = penalty.Recipient.CurrentAlias.Alias.UserName,
                CommunityName = penalty.Community.CommunityName,
                EvidenceMissing = string.IsNullOrWhiteSpace(penalty.Evidence) && penalty.PenaltyScope == PenaltyScope.Global
                                                                              && penalty.PenaltyStatus == PenaltyStatus.Active
                                                                              && !penalty.Automated,
                Automated = penalty.Automated
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Shared.Models.PenaltiesView.Penalty>
        {
            Data = pagedData,
            Count = count
        };
    }
}
