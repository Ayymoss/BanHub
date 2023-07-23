using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Commands.Players;
using BanHub.WebCore.Shared.Models.PlayersView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class PlayersService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IPlayersService _api;

    public PlayersService()
    {
        _api = RestClient.For<IPlayersService>(ApiHost);
    }

    public async Task<PlayerContext> GetPlayersAsync(GetPlayersPaginationCommand playersPagination)
    {
        try
        {
            var response = await _api.GetPlayersAsync(playersPagination);
            var result = await response.DeserializeHttpResponseContentAsync<PlayerContext>();
            return result ?? new PlayerContext();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get players pagination: {e.Message}");
        }

        return new PlayerContext();
    }
}
