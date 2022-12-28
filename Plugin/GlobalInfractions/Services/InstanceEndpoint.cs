using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class InstanceEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
    private const string ApiHost = "http://localhost:5000";

    public InstanceEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<bool> PostInstance(InstanceDto instance)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiHost}/api/Instance", instance);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error posting instance: {e.Message}");
        }

        return false;
    }

    public async Task<bool> IsInstanceActive(Guid guid)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiHost}/api/Instance/Active?instanceGuid={guid.ToString()}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error getting instance state: {e.Message}");
        }

        return false;
    }
}
