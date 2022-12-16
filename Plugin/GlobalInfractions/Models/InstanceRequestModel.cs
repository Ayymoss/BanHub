namespace GlobalInfractions.Models;

public class InstanceRequestModel
{
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
    /// The IW4MAdmin provided API Key
    /// </summary>
    public Guid ApiKey { get; set; }
}
