namespace BanHubData.SignalR;

public static class SignalRMethods
{
    public static class StatisticMethods
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
    }

    public static class TomatoMethods
    {
        /// <summary>
        /// TomatoCounterHub: Get current tomato count
        /// </summary>
        public const string GetTomatoCount = nameof(GetTomatoCount);

        /// <summary>
        /// TomatoCounterHub: Broadcast tomato count
        /// </summary>
        public const string OnTomatoCountUpdate = nameof(OnTomatoCountUpdate);

        /// <summary>
        /// TomatoCounterHub: Increment tomato count
        /// </summary>
        public const string IncrementTomatoCount = nameof(IncrementTomatoCount);
    }

    public static class ActiveMethods
    {
        /// <summary>
        /// ActiveUserHub: Get active users count
        /// </summary>
        public const string GetActiveUsersCount = nameof(GetActiveUsersCount);

        /// <summary>
        /// ActiveUserHub: Broadcast active users count
        /// </summary>
        public const string OnActiveUsersUpdate = nameof(OnActiveUsersUpdate);
    }

    public static class PluginMethods
    {
        /// <summary>
        /// PluginHub: Broadcast global ban
        /// </summary>
        public const string OnGlobalBan = nameof(OnGlobalBan);

        /// <summary>
        /// PluginHub: Activate community
        /// </summary>
        public const string ActivateCommunity = nameof(ActivateCommunity);

        /// <summary>
        /// PluginHub: Community heartbeat
        /// </summary>
        public const string CommunityHeartbeat = nameof(CommunityHeartbeat);

        /// <summary>
        /// PluginHub: Players heartbeat
        /// </summary>
        public const string PlayersHeartbeat = nameof(PlayersHeartbeat);

        /// <summary>
        /// PluginHub: Player joined
        /// </summary>
        public const string PlayerJoined = nameof(PlayerJoined);
    }
}
