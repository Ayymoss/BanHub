using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Commands.Community;
using BanHub.WebCore.Shared.Models.CommunityProfileView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;
using Community = BanHub.WebCore.Shared.Models.CommunitiesView.Community;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class CommunityService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly ICommunityService _api;

    public CommunityService()
    {
        _api = RestClient.For<ICommunityService>(ApiHost);
    }

    public async Task<IEnumerable<Community>> GetCommunitiesPaginationAsync(GetCommunitiesPaginationCommand communitiesPagination)
    {
        try
        {
            var response = await _api.GetCommunitiesPaginationAsync(communitiesPagination);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Community>>();
            return result ?? new List<Community>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new List<Community>();
    }

    public async Task<WebCore.Shared.Models.CommunityProfileView.Community> GetCommunityAsync(string identity)
    {
        try
        {
            var response = await _api.GetCommunityAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<WebCore.Shared.Models.CommunityProfileView.Community>();
            return result ?? new WebCore.Shared.Models.CommunityProfileView.Community();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new WebCore.Shared.Models.CommunityProfileView.Community();
    }
    
    public async Task<IEnumerable<Server>> GetCommunityProfileServersAsync(string identity)
    {
        try
        {
            var response = await _api.GetCommunityProfileServersAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Server>>();
            return result ?? new List<Server>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new List<Server>();
    }
    
    public async Task<bool> ToggleCommunityActivationAsync(string identity)
    {
        try
        {
            var response = await _api.ToggleCommunityActivationAsync(identity);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to toggle instance activation: {e.Message}");
        }

        return false;
    }
}
