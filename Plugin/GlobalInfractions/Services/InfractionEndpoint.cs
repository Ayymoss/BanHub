using System.Net.Http.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class InfractionEndpoint
{
    private readonly ConfigurationModel _config;
    private readonly HttpClient _httpClient = new();
    
    public InfractionEndpoint(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
        handler.BuildAsync();
        _config = handler.Configuration();
    }

    public async Task<bool> PostInfraction(InfractionDto infraction)
    {
        var response = await _httpClient.PostAsJsonAsync($"http://localhost:5000/api/Infraction?authToken={_config.ApiKey}", infraction);
        return response.IsSuccessStatusCode;
    }
}
