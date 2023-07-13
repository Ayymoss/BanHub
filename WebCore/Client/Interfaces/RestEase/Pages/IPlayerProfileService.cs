using BanHub.WebCore.Shared.Commands.PlayerProfile;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IPlayerProfileService
{
    [Get("/Player/Profile/{id}")]
    Task<HttpResponseMessage> GetProfileAsync([Query("identity")] string identity);

    [Get("/Player/Profile/Connections/{id}")]
    Task<HttpResponseMessage> GetConnectionsAsync([Query("identity")] string identity);
}
