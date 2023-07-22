using System.Text;
using BanHubData.Enums;

namespace BanHub.WebCore.Client.Utilities;

public static class HelperMethods
{
    public static string GetRolesAsString(IEnumerable<WebRole>? webRoles = null, IEnumerable<CommunityRole>? communityRoles = null)
    {
        var webRoleStr = (webRoles ?? Enumerable.Empty<WebRole>())
            .Select(role => $"Web_{role}");

        var communityRoleStr = (communityRoles ?? Enumerable.Empty<CommunityRole>())
            .Select(role => $"Community_{role}");

        return string.Join(", ", webRoleStr.Concat(communityRoleStr));
    }
}
