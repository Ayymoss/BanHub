namespace BanHubData.SignalR;

public static class HubMethods
{
    /// <summary>
    /// StatisticsHub: Get current online players count
    /// </summary>
    public const string GetCurrentOnlinePlayers = nameof(GetCurrentOnlinePlayers);

    /// <summary>
    /// StatisticsHub: Get current recent bans count
    /// </summary>
    public const string GetCurrentRecentBans = nameof(GetCurrentRecentBans);

    /// <summary>
    /// StatisticsHub: Broadcast online players count
    /// </summary>
    public const string OnPlayerCountUpdate = nameof(OnPlayerCountUpdate);

    /// <summary>
    /// StatisticsHub: Broadcast recent bans count
    /// </summary>
    public const string OnRecentBansUpdate = nameof(OnRecentBansUpdate);

    /// <summary>
    /// ActiveUserHub: Get active users count
    /// </summary>
    public const string GetActiveUsersCount = nameof(GetActiveUsersCount);

    /// <summary>
    /// ActiveUserHub: Broadcast active users count
    /// </summary>
    public const string OnActiveUsersUpdate = nameof(OnActiveUsersUpdate);

    /// <summary>
    /// PluginHub: Broadcast global ban
    /// </summary>
    public const string OnGlobalBan = nameof(OnGlobalBan);
}
