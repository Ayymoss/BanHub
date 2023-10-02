using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.Domain.Entities;

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
    public required DateTimeOffset Created { get; set; }

    /// <summary>
    /// The associated entity
    /// </summary>
    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; }
}
