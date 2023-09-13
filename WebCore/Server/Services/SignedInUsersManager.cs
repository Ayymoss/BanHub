using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Services;

public class SignedInUsersManager : ISignedInUsersManager
{
    private ConcurrentDictionary<string, WebUser> _users = new();

    // TODO: The user write/read needs to be updated in the future. This is a temporary solution.
    public void AddUser(WebUser user)
    {
        _users.TryAdd(user.SignedInGuid, user);

        if (_users.Count is 0) return;
        var serialized = JsonSerializer.Serialize(_users);
        File.WriteAllText(GetSignedInUsersPath(), serialized);
    }

    public bool IsUserInRole<TEnum>(string? signedInGuid, IEnumerable<TEnum> roles, Func<string, TEnum, bool> roleChecker)
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

    public WebUser? GetSignedInUser(string signedInGuid)
    {
        if (_users.Count is 0)
        {
            var fileRead = File.ReadAllText(GetSignedInUsersPath());
            var deserialized = JsonSerializer.Deserialize<ConcurrentDictionary<string, WebUser>>(fileRead);
            if (deserialized is not null) _users = deserialized;
        }

        return !_users.TryGetValue(signedInGuid, out var profile) ? null : profile;
    }

    private static string GetSignedInUsersPath()
    {
        var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Path.Join(workingDirectory, "Configuration", "SignedInUsers.json");
    }
}
