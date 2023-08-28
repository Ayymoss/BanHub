using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Note;

public class GetProfileNotesPaginationHandler : IRequestHandler<GetProfileNotesPaginationCommand,
    PaginationContext<Shared.Models.PlayerProfileView.Note>>
{
    private readonly DataContext _context;

    public GetProfileNotesPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Shared.Models.PlayerProfileView.Note>> Handle(GetProfileNotesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Notes
            .Where(x => x.Recipient.Identity == request.Identity)
            .Where(x => request.Privileged || !x.IsPrivate)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.Issuer.CurrentAlias.Alias.UserName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Message, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "Message" => current.ApplySort(sort, p => p.Message),
                "IssuerUserName" => current.ApplySort(sort, p => p.Issuer.CurrentAlias.Alias.UserName),
                "Created" => current.ApplySort(sort, p => p.Created),
                _ => current
            });

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(x => new Shared.Models.PlayerProfileView.Note
            {
                NoteGuid = x.NoteGuid,
                Message = x.Message,
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                AdminIdentity = x.Issuer.Identity,
                Created = x.Created,
                IsPrivate = x.IsPrivate
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Shared.Models.PlayerProfileView.Note>
        {
            Data = pagedData,
            Count = count
        };
    }
}
