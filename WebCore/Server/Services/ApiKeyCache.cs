using System.Collections.Concurrent;

namespace BanHub.WebCore.Server.Services;

public class ApiKeyCache
{
    public ConcurrentDictionary<Guid, Guid>? ApiKeys { get; set; }
}
