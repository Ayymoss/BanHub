using BanHub.WebCore.Shared.Commands.Web;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IAuthService
{
    [Post("/Auth/Login")]
    Task<HttpResponseMessage> LoginAsync([Body] WebTokenLoginCommand webToken);

    [Get("/Auth/Profile")]
    Task<HttpResponseMessage> GetProfileAsync();

    [Post("/Auth/Logout")]
    Task<HttpResponseMessage> LogoutAsync();
}
