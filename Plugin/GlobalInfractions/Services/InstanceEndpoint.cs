using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class InstanceEndpoint
{
    private readonly ConfigurationModel _config;
    private readonly HttpClient _httpClient = new();

    public InstanceEndpoint(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
        handler.BuildAsync();
        _config = handler.Configuration();
    }

    public async Task<bool> PostInstance(InstanceDto instance)
    {
        var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/api/Instance", instance);
        return response.IsSuccessStatusCode;
    }

    public async Task<InstanceDto?> GetInstance()
    {
        var response =
            await _httpClient.GetAsync($"http://localhost:5000/api/Instance?guid={Plugin.InfractionManager.Instance.InstanceGuid}");

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InstanceDto>(content);
    }
}
