using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Context;

/// <summary>
/// Table for all aliases 
/// </summary>
public class EFAlias
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The player's name
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// The player's IP address
    /// </summary>
    public required string IpAddress { get; set; }

    /// <summary>
    /// The last time the player's name changed
    /// </summary>
    public required DateTimeOffset Changed { get; set; }

    /// <summary>
    /// The associated entity
    /// </summary>
    public int EntityId { get; set; }

    [ForeignKey(nameof(EntityId))] public EFEntity Entity { get; set; } = null!;
}
