﻿using System.Collections.Concurrent;
using BanHub.Domain.ValueObjects.Plugin;

namespace BanHub.Plugin.Models;

public class CommunitySlim
{
    public ConcurrentDictionary<string, List<MessageContext>> PlayerMessages { get; set; } = new();
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public Guid ApiKey { get; set; }
    public bool Active { get; set; }
    public string PluginVersion { get; set; }
}

