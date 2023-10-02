using BanHub.Application.DTOs.WebView.SearchView;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Chat;
using MediatR;

namespace BanHub.Application.Mediatr.Search.Commands;

public class GetSearchHandler(IPlayerRepository playerRepository, IChatRepository chatRepository) : IRequestHandler<GetSearchCommand, (
    ControllerEnums.ReturnState State, DTOs.WebView.SearchView.Search? Search)>
{
    public async Task<(ControllerEnums.ReturnState State, DTOs.WebView.SearchView.Search? Search)> Handle(GetSearchCommand request,
        CancellationToken cancellationToken)
    {
        var players = await playerRepository.GetPlayerIdentitiesAsync(request.Query, cancellationToken);
        var chats = await chatRepository.GetSearchChatsAsync(request.Query, cancellationToken);

        var searchPlayers = players as SearchPlayer[] ?? players.ToArray();
        var searchChats = chats as SearchChat[] ?? chats.ToArray();

        if (searchChats.Length > 2_500 || searchPlayers.Length > 2_500)
            return (ControllerEnums.ReturnState.Ok, new DTOs.WebView.SearchView.Search
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

        var search = new DTOs.WebView.SearchView.Search
        {
            Players = searchPlayers.ToList(),
            Messages = searchChats.ToList()
        };

        return (ControllerEnums.ReturnState.Ok, search);
    }
}
