using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories.Pagination;

public class ProfileNotesPaginationQueryHelper(DataContext context) : IResourceQueryHelper<GetProfileNotesPaginationCommand, Note>
{
    public async Task<PaginationContext<Note>> QueryResourceAsync(GetProfileNotesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = context.Notes
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
            .Select(x => new Note
            {
                NoteGuid = x.NoteGuid,
                Message = x.Message,
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                AdminIdentity = x.Issuer.Identity,
                Created = x.Created,
                IsPrivate = x.IsPrivate
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Note>
        {
            Data = pagedData,
            Count = count
        };
    }
}
