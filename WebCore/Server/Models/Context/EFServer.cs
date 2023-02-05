using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Context;

public class EFServer
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The server Id
    /// </summary>
    public required string ServerId { get; set; }

    /// <summary>
    /// The server name
    /// </summary>
    public required string ServerName { get; set; }

    /// <summary>
    /// The server IP
    /// </summary>
    public required string ServerIp { get; set; }

    /// <summary>
    /// The port of the server
    /// </summary>
    public required int ServerPort { get; set; }

    /// <summary>
    /// Current alias associated with entity.
    /// </summary>
    public int InstanceId { get; set; }

    [ForeignKey(nameof(InstanceId))] public EFInstance Instance { get; set; } = null!;
}
