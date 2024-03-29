﻿namespace BanHub.Domain.ValueObjects.Plugin;

public class CreateOrUpdateCommunitySlim
{
    public Guid CommunityGuid { get; set; }
    public Guid CommunityApiKey { get; set; }
    public string CommunityName { get; set; }
    public string CommunityIp { get; set; }
    public string? CommunityWebsite { get; set; }
    public int CommunityPort { get; set; }
    public string About { get; set; }
    public Dictionary<string, string> Socials { get; set; }
}
