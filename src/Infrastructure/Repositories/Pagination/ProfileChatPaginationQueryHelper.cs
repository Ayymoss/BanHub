using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using BanHub.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories.Pagination;

public class ProfileChatPaginationQueryHelper(DataContext context) : IResourceQueryHelper<GetProfileChatPaginationCommand, Chat>
{
    public async Task<PaginationContext<Chat>> QueryResourceAsync(GetProfileChatPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = context.Chats
            .Where(x => x.Player.Identity == request.Identity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
            query = query.Where(search =>
                EF.Functions.ILike(search.Message, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Server.ServerName, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Community.CommunityName, $"%{request.SearchString}%"));

        if (request.Sorts.Any())
            query = request.Sorts.Aggregate(query, (current, sort) => sort.Property switch
            {
                "Message" => current.ApplySort(sort, p => p.Message),
                "CommunityName" => current.ApplySort(sort, p => p.Community.CommunityName),
                "Submitted" => current.ApplySort(sort, p => p.Submitted),
                "Server" => current.ApplySort(sort, p => p.Server.ServerName),
                _ => current
            });


        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Skip)
            .Take(request.Top)
            .Select(chat => new Chat
            {
                Message = chat.Message,
                Submitted = chat.Submitted,
                CommunityGuid = chat.Community.CommunityGuid,
                CommunityName = chat.Community.CommunityName,
                ServerId = chat.Server.ServerId,
                ServerName = chat.Server.ServerName
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Chat>
        {
            Data = pagedData,
            Count = count
        };
    }
}
