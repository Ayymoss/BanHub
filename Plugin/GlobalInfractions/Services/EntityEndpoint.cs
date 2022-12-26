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
    
    public async Task<bool> PostEntity(EntityDto entity)
    {
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5000/api/Entity?authToken={_config.ApiKey}", entity);
        return response.IsSuccessStatusCode;
    }
}
