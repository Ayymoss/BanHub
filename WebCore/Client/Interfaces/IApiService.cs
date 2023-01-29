using BanHub.WebCore.Shared.DTOs.WebEntity;

namespace BanHub.WebCore.Client.Interfaces;

public interface IApiService
{
    Task<string> LoginAsync(LoginRequestDto login);
    Task<(string, UserDto?)> UserProfileAsync();
    Task<string> LogoutAsync();
}
