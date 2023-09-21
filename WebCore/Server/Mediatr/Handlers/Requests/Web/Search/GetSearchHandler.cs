using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Search;
using BanHub.WebCore.Shared.Models.SearchView;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Search;

public class GetSearchHandler(DataContext context)
    : IRequestHandler<GetSearchCommand, (ControllerEnums.ReturnState State, Shared.Models.SearchView.Search? Search)>
{
    public async Task<(ControllerEnums.ReturnState State, Shared.Models.SearchView.Search? Search)> Handle(GetSearchCommand request,
        CancellationToken cancellationToken)
    {
        var players = await context.Players
            .Where(search =>
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{request.Query}%") ||
                EF.Functions.ILike(search.Identity, $"%{request.Query}%"))
            .Select(x => new SearchPlayer
            {
                Identity = x.Identity,
                Username = x.CurrentAlias.Alias.UserName
            }).ToListAsync(cancellationToken: cancellationToken);

        var chats = await context.Chats
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

        if (chats.Count > 2_500 || players.Count > 2_500)
            return (ControllerEnums.ReturnState.Ok, new Shared.Models.SearchView.Search
            {
                Players = new List<SearchPlayer>
                {
                    new()
                    {
                        Identity = "0:UKN",
                        Username = "Too many results"
                    }
                },
                Messages = new List<SearchChat>
                {
                    new()
                    {
                        Message = "Too many results",
                        Player = new SearchPlayer
                        {
                            Identity = "0:UKN",
                            Username = "Too many results"
                        }
                    }
                }
            });

        var search = new Shared.Models.SearchView.Search
        {
            Players = players,
            Messages = chats
        };

        return (ControllerEnums.ReturnState.Ok, search);
    }
}
