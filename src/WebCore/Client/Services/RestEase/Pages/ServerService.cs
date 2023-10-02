using BanHub.Application.Mediatr.Server.Commands;
using BanHub.Domain.ValueObjects.Services;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class ServerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IServerService _api = RestClient.For<IServerService>(ApiHost);

    public async Task<PaginationContext<Application.DTOs.WebView.ServersView.Server>> GetServersPaginationAsync(GetServersPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetServersPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Application.DTOs.WebView.ServersView.Server>>();
            return result ?? new PaginationContext<Application.DTOs.WebView.ServersView.Server>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get players pagination: {e.Message}");
        }

        return new PaginationContext<Application.DTOs.WebView.ServersView.Server>();
    }
}
