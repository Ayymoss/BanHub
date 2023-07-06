using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Context;

/// <summary>
/// Table for the user's current alias. 
/// </summary>
public class EFCurrentAlias
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// Entity associated with current alias.
    /// </summary>
    public int EntityId { get; set; }

    [ForeignKey(nameof(EntityId))] public EFPlayer Player { get; set; } = null!;

    /// <summary>
    /// Current alias associated with entity.
    /// </summary>
    public int AliasId { get; set; }

    [ForeignKey(nameof(AliasId))] public EFAlias Alias { get; set; } = null!;
}
