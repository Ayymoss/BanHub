using BanHub.WebCore.Shared.Commands.Community;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface ICommunityService
{
    [Post("/Community/Pagination")]
    Task<HttpResponseMessage> GetCommunitiesPaginationAsync([Body] GetCommunitiesPaginationCommand penaltiesPaginationQuery);

    [Get("/Community/{identity}")]
    Task<HttpResponseMessage> GetCommunityAsync([Path("identity")] string identity);

    [Get("/Community/Profile/Servers/{identity}")]
    Task<HttpResponseMessage> GetCommunityProfileServersAsync([Path("identity")] string identity);

    [Patch("/Community/Activation/{identity}")]
    Task<HttpResponseMessage> ToggleCommunityActivationAsync([Path("identity")] string identity);
}
