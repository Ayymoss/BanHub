namespace BanHub.Domain.ValueObjects.Plugin;

public class IsPlayerBannedCommandSlim
{
    public string Identity { get; set; }
    public string IpAddress { get; set; }
}
