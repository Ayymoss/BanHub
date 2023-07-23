using System.Text.RegularExpressions;

namespace BanHub.Utilities;

public class HelperMethods
{
    public static string ObscureGuid(string input)
    {
        var guidPattern = @"(\b[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}\b)";

        return Regex.Replace(input, guidPattern, m => 
            $"{m.Value[..8]}-****-****-****-{m.Value[24..]}");
    }
}
