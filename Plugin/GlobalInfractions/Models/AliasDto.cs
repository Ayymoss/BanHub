namespace GlobalInfractions.Models;

public class AliasDto
{
    /// <summary>
    /// The player's name
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// The player's IP address
    /// </summary>
    public string IpAddress { get; set; } = null!;
    
    /// <summary>
    /// The last time the player's name changed
    /// </summary>
    public DateTimeOffset Changed { get; set; }
}
