using BanHubData.Mediatr.Commands.Requests.Community;
using RestEase;

namespace BanHub.Interfaces;

public interface ICommunityService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Community")]
    Task<HttpResponseMessage> CreateOrUpdateCommunityAsync([Body] CreateOrUpdateCommunityCommand community);

    [Get("/Community/Active/{identity}")]
    Task<HttpResponseMessage> IsCommunityActiveAsync([Path("identity")] string identity);
}
