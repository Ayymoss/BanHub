using System.Collections.Concurrent;
using BanHubData.Mediatr.Commands.Requests.Chat;

namespace BanHub.Models;

public class CommunitySlim
{
    public ConcurrentDictionary<string, List<MessageContext>> PlayerMessages { get; set; } = new();
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public Guid ApiKey { get; set; }
    public bool Active { get; set; }
}

