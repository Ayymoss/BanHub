using BanHub.Domain.ValueObjects.Plugin;
using RestEase;

namespace BanHub.Plugin.Interfaces;

public interface IServerService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Server")]
    Task<HttpResponseMessage> CreateOrUpdateServerAsync([Body] CreateOrUpdateServerCommandSlim server);
}
