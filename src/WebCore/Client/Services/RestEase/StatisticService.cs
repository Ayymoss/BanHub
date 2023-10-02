using BanHub.Application.DTOs.WebView.Shared;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase;

public class StatisticService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IStatisticService _api = RestClient.For<IStatisticService>(ApiHost);

    public async Task<Statistic> GetStatisticAsync()
    {
        try
        {
            var response = await _api.GetStatisticAsync();
            var result = await response.DeserializeHttpResponseContentAsync<Statistic>();
            return result ?? new Statistic();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get agent pagination: {e.Message}");
        }

        return new Statistic();
    }
}
