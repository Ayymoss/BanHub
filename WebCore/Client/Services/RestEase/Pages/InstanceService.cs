using BanHub.WebCore.Client.Interfaces.RestEase.Pages;
using BanHub.WebCore.Shared.Commands.Instance;
using BanHub.WebCore.Shared.Models.InstanceProfileView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;
using Instance = BanHub.WebCore.Shared.Models.InstancesView.Instance;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class InstanceService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "http://banhub.gg:8123/api";
#endif
    private readonly IInstanceService _api;

    public InstanceService()
    {
        _api = RestClient.For<IInstanceService>(ApiHost);
    }

    public async Task<IEnumerable<Instance>> GetInstancesPaginationAsync(GetInstancesPaginationCommand instancesPagination)
    {
        try
        {
            var response = await _api.GetInstancesPaginationAsync(instancesPagination);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Instance>>();
            return result ?? new List<Instance>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new List<Instance>();
    }

    public async Task<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance> GetInstanceAsync(string identity)
    {
        try
        {
            var response = await _api.GetInstanceAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance>();
            return result ?? new BanHub.WebCore.Shared.Models.InstanceProfileView.Instance();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new BanHub.WebCore.Shared.Models.InstanceProfileView.Instance();
    }
    
    public async Task<IEnumerable<Server>> GetInstanceProfileServersAsync(string identity)
    {
        try
        {
            var response = await _api.GetInstanceProfileServersAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Server>>();
            return result ?? new List<Server>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get instances: {e.Message}");
        }

        return new List<Server>();
    }
    
    public async Task<bool> ToggleInstanceActivationAsync(string identity)
    {
        try
        {
            var response = await _api.ToggleInstanceActivationAsync(identity);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to toggle instance activation: {e.Message}");
        }

        return false;
    }
}
