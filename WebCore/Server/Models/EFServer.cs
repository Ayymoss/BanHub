using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalInfraction.WebCore.Server.Models;

public class EFServer
{
    
    [Key] public int Id { get; set; }

    /// <summary>
    /// The server Id
    /// </summary>
    public string ServerId { get; set; } = null!;
    
    /// <summary>
    /// The server name
    /// </summary>
    public string ServerName { get; set; } = null!;
    
    /// <summary>
    /// The server IP
    /// </summary>
    public string ServerIp { get; set; } = null!;

    /// <summary>
    /// The port of the server
    /// </summary>
    public int ServerPort { get; set; }
    
    /// <summary>
    /// Current alias associated with entity.
    /// </summary>
    public int InstanceId { get; set; }
    [ForeignKey(nameof(InstanceId))] public EFInstance Instance { get; set; } = null!;
}
