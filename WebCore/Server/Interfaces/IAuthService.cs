using System.Security.Claims;
using Data.Enums;
using Data.Domains.WebEntity;
using Microsoft.AspNetCore.Authentication;

namespace BanHub.WebCore.Server.Interfaces;

public interface IAuthService
{
    Task<(ControllerEnums.ReturnState, ClaimsIdentity, AuthenticationProperties)> LoginAsync(WebLoginRequest webLoginRequest);
    Task<WebUser?> UserProfileAsync(int userId);
}
