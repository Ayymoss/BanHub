using BanHub.WebCore.Shared.Commands.Web;
using BanHub.WebCore.Shared.Models.Shared;

namespace BanHub.WebCore.Client.Interfaces;

public interface IApiService
{
    Task<string> LoginAsync(WebTokenLoginCommand webLogin);
    Task<(string, WebUser?)> UserProfileAsync();
    Task<string> LogoutAsync();
}
