namespace BanHub.Domain.ValueObjects.Services;


// TODO: Check usages?
public class ServerOnlineStatistic
{
    public int OnlineCount { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
