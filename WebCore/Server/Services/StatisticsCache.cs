using System.Collections.Concurrent;

namespace BanHub.WebCore.Server.Services;

public class StatisticsCache
{
    public bool Loaded { get; set; }
    public int Penalties;
    public int Servers;
    public int Communities;
    public int Players;
    public ConcurrentDictionary<Guid, DateTimeOffset> RecentBans { get; set; } = new();
    public ConcurrentDictionary<string, DateTimeOffset> OnlinePlayers { get; set; } = new();
}
