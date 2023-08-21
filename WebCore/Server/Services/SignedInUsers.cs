using System.Collections.Concurrent;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Services;

public class SignedInUsers
{
    private readonly ConcurrentDictionary<string, WebUser> _users = new();

    public void AddUser(WebUser user) => _users.TryAdd(user.SignedInGuid, user);

    public static bool IsUserInRole<TEnum>(string? signedInGuid, IEnumerable<TEnum> roles, Func<string, TEnum, bool> roleChecker)
        where TEnum : Enum => signedInGuid is not null && roles.Any(role => roleChecker(signedInGuid, role));

    public bool IsUserInWebRole(string signedInGuid, WebRole role)
    {
        _users.TryGetValue(signedInGuid, out var user);
        return user is not null && role.ToString() == user.WebRole;
    }

    public bool IsUserInCommunityRole(string signedInGuid, CommunityRole role)
    {
        _users.TryGetValue(signedInGuid, out var user);
        return user is not null && role.ToString() == user.CommunityRole;
    }

    public WebUser? GetSignedInUser(string signedInGuid) =>
        !_users.TryGetValue(signedInGuid, out var profile) ? null : profile;
}
