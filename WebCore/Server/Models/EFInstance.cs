using System.ComponentModel.DataAnnotations;

namespace GlobalInfraction.WebCore.Server.Models;

/// <summary>
/// Table for the all instances
/// </summary>
public class EFInstance
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The IW4MAdmin GUID
    /// </summary>
    public Guid InstanceGuid { get; set; }

    /// <summary>
    /// The IW4MAdmin IP address
    /// </summary>
    public string InstanceIp { get; set; } = null!;

    /// <summary>
    /// The IW4MAdmin name
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// The last the the instance has replied
    /// </summary>
    public DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// The IW4MAdmin provided API Key
    /// </summary>
    public Guid ApiKey { get; set; }

    /// <summary>
    /// State whether the server can upload bans
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// The list of connected servers
    /// </summary>
    public virtual ICollection<EFServer> ServerConnections { get; set; } = null!;
}
