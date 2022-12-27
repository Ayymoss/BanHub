using System.Net.Http.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class HeartBeatEndpoint
{
    private readonly ConfigurationModel _config;
    private readonly HttpClient _httpClient = new();

    public HeartBeatEndpoint(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
        handler.BuildAsync();
        _config = handler.Configuration();
    }

    public async Task<bool> PostInstanceHeartBeat(InstanceDto instance)
    {
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5000/api/HeartBeat/Instance", instance);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> PostEntityHeartBeat(List<EntityDto> entity)
    {
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5000/api/HeartBeat/Entities?authToken={_config.ApiKey}", entity);
        return response.IsSuccessStatusCode;
    }
}
