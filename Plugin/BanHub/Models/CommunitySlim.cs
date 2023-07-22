using System.Collections.Concurrent;

namespace BanHub.Models;

public class CommunitySlim
{
    public ConcurrentDictionary<string, List<(DateTimeOffset Submitted, string Message)>> PlayerMessages { get; set; } = new();
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public Guid ApiKey { get; set; }
    public bool Active { get; set; }
}
