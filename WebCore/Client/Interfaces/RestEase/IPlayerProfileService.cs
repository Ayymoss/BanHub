using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IPlayerProfileService
{
    [Get("/Player/Profile/{identity}")]
    Task<HttpResponseMessage> GetProfileAsync([Path("identity")] string identity);

    [Get("/Player/Profile/Connections/{identity}")]
    Task<HttpResponseMessage> GetProfileConnectionsAsync([Path("identity")] string identity);
}
