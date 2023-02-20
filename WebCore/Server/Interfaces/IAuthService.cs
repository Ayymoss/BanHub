using System.Security.Claims;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs.WebEntity;
using Microsoft.AspNetCore.Authentication;

namespace BanHub.WebCore.Server.Interfaces;

public interface IAuthService
{
    Task<(ControllerEnums.ReturnState, ClaimsIdentity, AuthenticationProperties)> LoginAsync(LoginRequestDto loginRequest);
    Task<UserDto?> UserProfileAsync(int userId);
}
