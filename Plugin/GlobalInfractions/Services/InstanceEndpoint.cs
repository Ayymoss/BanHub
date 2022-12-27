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
    
    public async Task<bool> IsInstanceActive(Guid guid)
    {
        var response = await _httpClient.GetAsync($"http://localhost:5000/api/Instance/Active?instanceGuid={guid.ToString()}");
        return response.IsSuccessStatusCode;

    }
}
