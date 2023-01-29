using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models;

/// <summary>
/// Table for all aliases 
/// </summary>
public class EFAlias
{
    [Key] public int Id { get; set; }

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

    /// <summary>
    /// The associated entity
    /// </summary>
    public int EntityId { get; set; } 
    [ForeignKey(nameof(EntityId))] public EFEntity Entity { get; set; } = null!;

}
