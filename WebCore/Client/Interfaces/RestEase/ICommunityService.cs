using BanHub.WebCore.Shared.Mediatr.Commands.Chat;
using BanHub.WebCore.Shared.Mediatr.Commands.Community;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface ICommunityService
{
    [Post("/Community/Pagination")]
    Task<HttpResponseMessage> GetCommunitiesPaginationAsync([Body] GetCommunitiesPaginationCommand penaltiesPaginationQuery);

    [Get("/Community/{identity}")]
    Task<HttpResponseMessage> GetCommunityAsync([Path("identity")] string identity);

    [Patch("/Community/Activation/{identity}")]
    Task<HttpResponseMessage> ToggleCommunityActivationAsync([Path("identity")] string identity);

    [Post("/Community/Profile/Servers")]
    Task<HttpResponseMessage> GetCommunityProfileServersPaginationAsync([Body] GetCommunityProfileServersPaginationCommand pagination);

    [Post("/Community/Profile/Penalties")]
    Task<HttpResponseMessage> GetCommunityProfilePenaltiesPaginationAsync([Body] GetCommunityProfilePenaltiesPaginationCommand paginationQuery);
}
