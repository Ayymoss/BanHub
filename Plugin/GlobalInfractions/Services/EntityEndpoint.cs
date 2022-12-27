using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class EntityEndpoint
{
    private readonly ConfigurationModel _config;
    private readonly HttpClient _httpClient = new();

    public EntityEndpoint(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
        handler.BuildAsync();
        _config = handler.Configuration();
    }

    public async Task<EntityDto?> GetEntity(string identity)
    {
        var response = await _httpClient.GetAsync($"http://localhost:5000/api/Entity?identity={identity}");

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<EntityDto>(content);
    }

    public async Task<bool> UpdateEntity(EntityDto entity)
    {
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5000/api/Entity?authToken={_config.ApiKey}", entity);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> HasEntity(string identity)
    {
        var response = await _httpClient.GetAsync($"http://localhost:5000/api/Entity/Exists?identity={identity}");
        if (!response.IsSuccessStatusCode) return false;
        var content = await response.Content.ReadAsStringAsync();
        var boolParse = bool.TryParse(content, out var result);
        return boolParse && result;
    }
}
