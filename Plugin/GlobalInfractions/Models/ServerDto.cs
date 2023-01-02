namespace GlobalInfractions.Models;

public class ServerDto
{
    /// <summary>
    /// The server Id
    /// </summary>
    public string ServerId { get; set; } = null!;
    
    /// <summary>
    /// The server the client connected to
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// The server IP the client connected to
    /// </summary>
    public string? ServerIp { get; set; }
    
    /// <summary>
    /// The port of the server
    /// </summary>
    public int? ServerPort { get; set; }

    /// <summary>
    /// When the connection happened
    /// </summary>
    public DateTimeOffset? Connected { get; set; }
    
    /// <summary>
    /// Information related to the server the infraction was issued from
    /// </summary>
    public InstanceDto? Instance { get; set; }

    
}
