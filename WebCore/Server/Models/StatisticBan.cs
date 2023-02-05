namespace BanHub.WebCore.Server.Models;

public class StatisticBan
{
    public Guid BanGuid { get; set; }
    public DateTimeOffset Submitted { get; set; }
}
