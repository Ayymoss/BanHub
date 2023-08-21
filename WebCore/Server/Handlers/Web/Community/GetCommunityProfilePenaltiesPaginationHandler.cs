using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Commands.Community;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Community;

public class GetCommunityProfilePenaltiesPaginationHandler : IRequestHandler<GetCommunityProfilePenaltiesPaginationCommand,
    PaginationContext<Shared.Models.CommunityProfileView.Penalty>>
{
    private readonly DataContext _context;

    public GetCommunityProfilePenaltiesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Shared.Models.CommunityProfileView.Penalty>> Handle(
        GetCommunityProfilePenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Penalties
            .Where(x => x.Community.CommunityGuid == request.Identity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.PenaltyGuid.ToString(), $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Reason, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Recipient.CurrentAlias.Alias.UserName, $"%{request.SearchString}%")||
                EF.Functions.ILike(search.Issuer.CurrentAlias.Alias.UserName, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "IssuerUserName" => current.ApplySort(sort, p => p.Issuer.CurrentAlias.Alias.UserName),
                "RecipientUserName" => current.ApplySort(sort, p => p.Recipient.CurrentAlias.Alias.UserName),
                "Reason" => current.ApplySort(sort, p => p.Reason),
                "PenaltyType" => current.ApplySort(sort, p => p.PenaltyType),
                "PenaltyStatus" => current.ApplySort(sort, p => p.PenaltyStatus),
                "PenaltyScope" => current.ApplySort(sort, p => p.PenaltyScope),
                "Submitted" => current.ApplySort(sort, p => p.Submitted),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(x => new Shared.Models.CommunityProfileView.Penalty
            {
                PenaltyGuid = x.PenaltyGuid,
                RecipientIdentity = x.Recipient.Identity,
                IssuerIdentity = x.Issuer.Identity,
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                Reason = request.Privileged && x.Automated
                    ? x.Reason
                    : x.Automated
                        ? "Automated Detection"
                        : x.Reason,
                Evidence = x.Evidence,
                RecipientUserName = x.Recipient.CurrentAlias.Alias.UserName,
                CommunityGuid = x.Community.CommunityGuid,
                Expiration = x.Expiration,
                PenaltyType = x.PenaltyType,
                PenaltyScope = x.PenaltyScope,
                PenaltyStatus = x.PenaltyStatus,
                Submitted = x.Submitted
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Shared.Models.CommunityProfileView.Penalty>
        {
            Data = pagedData,
            Count = count
        };
    }
}
