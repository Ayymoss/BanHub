using BanHub.Domain.ValueObjects.Plugin;
using RestEase;

namespace BanHub.Plugin.Interfaces;

public interface ICommunityService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Community")]
    Task<HttpResponseMessage> CreateOrUpdateCommunityAsync([Body] CreateOrUpdateCommunitySlim communitySlim);

    [Get("/Community/Active/{identity}")]
    Task<HttpResponseMessage> IsCommunityActiveAsync([Path("identity")] string identity);
}
