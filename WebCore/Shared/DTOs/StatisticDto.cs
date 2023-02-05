namespace BanHub.WebCore.Shared.DTOs;

public class StatisticDto
{
    /// <summary>
    /// The count of how many entities there are
    /// </summary>
    public int? EntityCount { get; set; }

    /// <summary>
    /// The count of how many instances there are
    /// </summary>
    public int? InstanceCount { get; set; }

    /// <summary>
    /// The count of how many servers there are
    /// </summary>
    public int? ServerCount { get; set; }

    /// <summary>
    /// The count of how many infractions there are
    /// </summary>
    public int? PenaltyCount { get; set; }

    /// <summary>
    /// The count of how many users are online
    /// </summary>
    public int OnlineCount { get; set; }

    /// <summary>
    /// The count of how many bans there are in last 24hr
    /// </summary>
    public int BanCount { get; set; }
}
