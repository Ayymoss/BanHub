﻿using System.Net.Http.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class InfractionEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
    private const string ApiHost = "http://localhost:5000";

    public InfractionEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<bool> PostInfraction(InfractionDto infraction)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/api/Infraction?authToken={_configurationModel.ApiKey}", infraction);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error sending instance heartbeat: {e.Message}");
        }

        return false;
    }
}
