using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BanHub.WebCore.Server.Handlers.Web.Chat;

public class GetChatPaginationHandler : IRequestHandler<GetChatPaginationCommand, ChatContext>
{
    private readonly DataContext _context;

    public GetChatPaginationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<ChatContext> Handle(GetChatPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var query = _context.Chats
            .Where(x => x.Player.Identity == request.PlayerIdentity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            query = query.Where(search =>
                EF.Functions.ILike(search.Message, $"%{request.SearchString}%") ||
                EF.Functions.ILike(search.Community.CommunityName, $"%{request.SearchString}%"));
        }

        query = request.SortLabel switch
        {
            "Message" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Message),
            "CommunityName" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Community.CommunityName),
            "Submitted" => query.OrderByDirection((SortDirection)request.SortDirection, key => key.Submitted),
            _ => query
        };

        var count = await query.CountAsync(cancellationToken: cancellationToken);
        var pagedData = await query
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(chat => new Shared.Models.PlayerProfileView.Chat
            {
                Message = chat.Message,
                Submitted = chat.Submitted,
                CommunityGuid = chat.Community.CommunityGuid,
                CommunityName = chat.Community.CommunityName
            }).ToListAsync(cancellationToken: cancellationToken);

        return new ChatContext
        {
            Chats = pagedData,
            Count = count
        };
    }
}
