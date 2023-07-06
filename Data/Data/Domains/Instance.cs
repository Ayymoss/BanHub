namespace Data.Domains;

public class Instance
{
    /// <summary>
    /// The IW4MAdmin GUID
    /// </summary>
    public Guid InstanceGuid { get; set; }

    /// <summary>
    /// The IW4MAdmin IP address
    /// </summary>
    public string InstanceIp { get; set; }

    /// <summary>
    /// The IW4MAdmin name
    /// </summary>
    public string InstanceName { get; set; }

    /// <summary>
    /// The last the the instance has replied
    /// </summary>
    public DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// When the instance was created
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// The IW4MAdmin provided API Key
    /// </summary>
    public Guid ApiKey { get; set; }

    /// <summary>
    /// Community description
    /// </summary>
    public string About { get; set; }

    /// <summary>
    /// Community socials
    /// </summary>
    public Dictionary<string, string> Socials { get; set; }

    /// <summary>
    /// State whether the server can upload bans
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Count of child servers
    /// </summary>
    public int ServerCount { get; set; }

    /// <summary>
    /// The list of servers the instance has
    /// </summary>
    public virtual ICollection<Server> Servers { get; set; }

    /// <summary>
    /// A server reference to this instance
    /// </summary>
    public virtual Server Server { get; set; }
}
