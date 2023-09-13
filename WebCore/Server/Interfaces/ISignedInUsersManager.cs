using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface ISignedInUsersManager
{
    void AddUser(WebUser user);
    bool IsUserInRole<TEnum>(string? signedInGuid, IEnumerable<TEnum> roles, Func<string, TEnum, bool> roleChecker) where TEnum : Enum;
    bool IsUserInWebRole(string signedInGuid, WebRole role);
    bool IsUserInCommunityRole(string signedInGuid, CommunityRole role);
    WebUser? GetSignedInUser(string signedInGuid);
}
