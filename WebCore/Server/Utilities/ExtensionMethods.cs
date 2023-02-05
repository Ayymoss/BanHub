using System.Security.Principal;

namespace BanHub.WebCore.Server.Utilities;

public static class ExtensionMethods
{
    public static bool IsInAnyRole(this IPrincipal principal, params string[] roles)
    {
        return roles.Any(principal.IsInRole);
    }
}
