using System.Net.Http.Json;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Models;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class EntityEndpoint
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IEntityService _api;
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine(
                    $"[{ConfigurationModel.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public EntityEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _api = RestClient.For<IEntityService>(ApiHost);
    }

    public async Task<EntityDto?> GetEntity(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.GetEntity(identity);
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<EntityDto>();
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error getting entity: {e.Message}");
        }

        return null;
    }

    public async Task<string?> GetToken(EntityDto entity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.GetToken(entity, _configurationModel.ApiKey.ToString());
                if (!response.IsSuccessStatusCode) return null;
                var result = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(result) ? null : result;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error getting token: {e.Message}");
        }

        return null;
    }

    public async Task<bool> UpdateEntity(EntityDto entity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.UpdateEntity(entity, _configurationModel.ApiKey.ToString());
                if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
                {
                    Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting evidence {entity.Identity}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(entity)}\n" +
                                      $"[{ConfigurationModel.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error updating entity: {e.Message}");
        }

        return false;
    }
}
