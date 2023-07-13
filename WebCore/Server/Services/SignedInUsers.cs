using System.Collections.Concurrent;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Services;

public class SignedInUsers
{
    private readonly ConcurrentDictionary<string, WebUser> _users;

    public SignedInUsers()
    {
        _users = new ConcurrentDictionary<string, WebUser>();
    }

    public void AddUser(WebUser user) => _users.TryAdd(user.SignedInGuid, user);

    public bool IsUserInAnyWebRole(string signedInGuid, IEnumerable<WebRole> role)
    {
        _users.TryGetValue(signedInGuid, out var user);
        return user is not null && role.Any(x => x.ToString() == user.WebRole);
    }

    public bool IsUserInAnyInstanceRole(string signedInGuid, IEnumerable<InstanceRole> role)
    {
        _users.TryGetValue(signedInGuid, out var user);
        return user is not null && role.Any(x => x.ToString() == user.InstanceRole);
    }

    public bool IsUserSignedIn(string signedInGuid) => _users.ContainsKey(signedInGuid);
}
