using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IPlayerProfileService
{
    [Get("/Player/Profile/{identity}")]
    Task<HttpResponseMessage> GetProfileAsync([Query("identity")] string identity);

    [Get("/Player/Profile/Connections/{identity}")]
    Task<HttpResponseMessage> GetProfileConnectionsAsync([Query("identity")] string identity);
}
