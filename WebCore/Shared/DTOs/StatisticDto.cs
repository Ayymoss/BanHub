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
    public int? InfractionCount { get; set; }
}
