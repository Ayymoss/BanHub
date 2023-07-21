using BanHubData.Commands.Instance;
using RestEase;

namespace BanHub.Interfaces;

public interface ICommunityService
{
    [Post("/Community")]
    Task<HttpResponseMessage> CreateOrUpdateCommunityAsync([Body] CreateOrUpdateCommunityCommand community);

    [Get("/Community/Active/{identity}")]
    Task<HttpResponseMessage> IsCommunityActiveAsync([Path("identity")] string identity);
}
