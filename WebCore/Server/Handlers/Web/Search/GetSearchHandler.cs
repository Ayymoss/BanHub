using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Search;
using BanHub.WebCore.Shared.Models.SearchView;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Search;

public class GetSearchHandler : IRequestHandler<GetSearchCommand, (ControllerEnums.ReturnState State, Shared.Models.SearchView.Search? Search)>
{
    private readonly DataContext _context;

    public GetSearchHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<(ControllerEnums.ReturnState State, Shared.Models.SearchView.Search? Search)> Handle(GetSearchCommand request,
        CancellationToken cancellationToken)
    {
        var players = await _context.Players
            .Where(search =>
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{request.Query}%") ||
                EF.Functions.ILike(search.Identity, $"%{request.Query}%"))
            .Select(x => new SearchPlayer
            {
                Identity = x.Identity,
                Username = x.CurrentAlias.Alias.UserName
            }).ToListAsync(cancellationToken: cancellationToken);

        var chats = await _context.Chats
            .Where(search => EF.Functions.ILike(search.Message, $"%{request.Query}%"))
            .Select(x => new SearchChat
            {
                Message = x.Message,
                Player = new SearchPlayer
                {
                    Identity = x.Player.Identity,
                    Username = x.Player.CurrentAlias.Alias.UserName
                }
            }).ToListAsync(cancellationToken: cancellationToken);

        if (chats.Count > 1_000 || players.Count > 1_000) return (ControllerEnums.ReturnState.BadRequest, null);

        var search = new Shared.Models.SearchView.Search
        {
            Players = players,
            Messages = chats
        };

        return (ControllerEnums.ReturnState.Ok, search);
    }
}
