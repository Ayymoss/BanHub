using System.ComponentModel.DataAnnotations;

namespace BanHub.WebCore.Server.Models.Domains;

/// <summary>
/// Table for the all instances
/// </summary>
public class EFInstance
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The IW4MAdmin GUID
    /// </summary>
    public required Guid InstanceGuid { get; set; }

    /// <summary>
    /// The IW4MAdmin IP address
    /// </summary>
    public required string InstanceIp { get; set; } = null!;

    /// <summary>
    /// The IW4MAdmin port
    /// </summary>
    public int InstancePort { get; set; } = 1624;

    /// <summary>
    /// The friendly community address
    /// </summary>
    public string? InstanceIpFriendly { get; set; }

    /// <summary>
    /// The IW4MAdmin name
    /// </summary>
    public required string InstanceName { get; set; }

    /// <summary>
    /// The last the the instance has replied
    /// </summary>
    public required DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// Community description
    /// </summary>
    public required string? About { get; set; }

    /// <summary>
    /// Community socials
    /// </summary>
    public required Dictionary<string, string>? Socials { get; set; }

    /// <summary>
    /// When the instance was created
    /// </summary>
    public required DateTimeOffset Created { get; set; }

    /// <summary>
    /// The IW4MAdmin provided API Key
    /// </summary>
    public required Guid ApiKey { get; set; }

    /// <summary>
    /// State whether the server can upload bans
    /// </summary>
    public required bool Active { get; set; }

    /// <summary>
    /// The list of connected servers
    /// </summary>
    public ICollection<EFServer> ServerConnections { get; set; } = null!;
}
