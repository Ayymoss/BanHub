using Data.Domains.WebEntity;

namespace BanHub.WebCore.Client.Interfaces;

public interface IApiService
{
    Task<string> LoginAsync(WebLoginRequest webLogin);
    Task<(string, WebUser?)> UserProfileAsync();
    Task<string> LogoutAsync();
}
