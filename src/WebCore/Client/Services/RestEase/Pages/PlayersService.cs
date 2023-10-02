using System.Text.Json;
using BanHub.Application.DTOs.WebView.PlayersView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.ValueObjects.Services;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class PlayersService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IPlayersService _api = RestClient.For<IPlayersService>(ApiHost);

    public async Task<PaginationContext<Player>> GetPlayersPaginationAsync(GetPlayersPaginationCommand playersPagination)
    {
        try
        {
            var response = await _api.GetPlayersAsync(playersPagination);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Player>>();
            return result ?? new PaginationContext<Player>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get players pagination: {e.Message}");
        }

        return new PaginationContext<Player>();
    }
}
