using BanHubData.Mediatr.Commands.Requests.Server;
using RestEase;

namespace BanHub.Interfaces;

public interface IServerService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Server")]
    Task<HttpResponseMessage> CreateOrUpdateServerAsync([Body] CreateOrUpdateServerCommand server);
}
