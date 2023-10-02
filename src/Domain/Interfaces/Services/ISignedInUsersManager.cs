using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Services;

namespace BanHub.Domain.Interfaces.Services;

public interface ISignedInUsersManager
{
    void AddUser(WebUser user);
    bool IsUserInRole<TEnum>(string? signedInGuid, IEnumerable<TEnum> roles, Func<string, TEnum, bool> roleChecker) where TEnum : Enum;
    bool IsUserInWebRole(string signedInGuid, WebRole role);
    bool IsUserInCommunityRole(string signedInGuid, CommunityRole role);
    WebUser? GetSignedInUser(string signedInGuid);
}
