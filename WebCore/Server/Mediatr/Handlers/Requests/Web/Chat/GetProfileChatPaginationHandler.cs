using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Chat;

public class GetProfileChatPaginationHandler : IRequestHandler<GetProfileChatPaginationCommand,
    PaginationContext<Shared.Models.PlayerProfileView.Chat>>
{
    private readonly DataContext _context;

    public GetProfileChatPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<PaginationContext<Shared.Models.PlayerProfileView.Chat>> Handle(GetProfileChatPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Chats
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
            .Select(chat => new Shared.Models.PlayerProfileView.Chat
            {
                Message = chat.Message,
                Submitted = chat.Submitted,
                CommunityGuid = chat.Community.CommunityGuid,
                CommunityName = chat.Community.CommunityName,
                ServerId = chat.Server.ServerId,
                ServerName = chat.Server.ServerName
            }).ToListAsync(cancellationToken: cancellationToken);

        return new PaginationContext<Shared.Models.PlayerProfileView.Chat>
        {
            Data = pagedData,
            Count = count
        };
    }
}
