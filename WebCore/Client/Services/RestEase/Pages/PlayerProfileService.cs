using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;


public class PlayerProfileService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IPlayerProfileService _api = RestClient.For<IPlayerProfileService>(ApiHost);

    public async Task<Player> GetProfileAsync(string identity)
    {
        try
        {
            var response = await _api.GetProfileAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<Player>();
            return result ?? new Player();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player profile: {e.Message}");
        }

        return new Player();
    }

    public async Task<IEnumerable<Connection>> GetConnectionsAsync(string identity)
    {
        try
        {
            var response = await _api.GetProfileConnectionsAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Connection>>();
            return result ?? new List<Connection>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player connections: {e.Message}");
        }

        return new List<Connection>();
    }

    
    

}
