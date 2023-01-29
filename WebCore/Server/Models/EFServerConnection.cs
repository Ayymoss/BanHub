using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models;

public class EFServerConnection
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The time they connected
    /// </summary>
    public DateTimeOffset Connected { get; set; }

    /// <summary>
    /// Entity associated with the connection
    /// </summary>
    public int EntityId { get; set; }
    [ForeignKey(nameof(EntityId))] public EFEntity Entity { get; set; } = null!;
    
    /// <summary>
    /// The server they connected to
    /// </summary>
    public int ServerId { get; set; }
    [ForeignKey(nameof(ServerId))] public EFServer Server { get; set; } = null!;
}
