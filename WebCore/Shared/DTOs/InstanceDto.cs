namespace GlobalInfraction.WebCore.Shared.DTOs;

public class InstanceDto
{
    /// <summary>
    /// The IW4MAdmin GUID
    /// </summary>
    public Guid InstanceGuid { get; set; }

    /// <summary>
    /// The IW4MAdmin IP address
    /// </summary>
    public string? InstanceIp { get; set; }

    /// <summary>
    /// The IW4MAdmin name
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// The last the the instance has replied
    /// </summary>
    public DateTimeOffset? HeartBeat { get; set; }
    
    /// <summary>
    /// The IW4MAdmin provided API Key
    /// </summary>
    public Guid? ApiKey { get; set; }

    /// <summary>
    /// State whether the server can upload bans
    /// </summary>
    public bool? Active { get; set; }
    
    /// <summary>
    /// The list of servers the instance has
    /// </summary>
    public virtual ICollection<ServerDto>? Servers { get; set; }
    
    /// <summary>
    /// A server reference to this instance
    /// </summary>
    public virtual ServerDto? Server { get; set; }
}
