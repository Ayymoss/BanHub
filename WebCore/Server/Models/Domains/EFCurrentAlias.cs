using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Domains;

/// <summary>
/// Table for the user's current alias. 
/// </summary>
public class EFCurrentAlias
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// Entity associated with current alias.
    /// </summary>
    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; }

    /// <summary>
    /// Current alias associated with entity.
    /// </summary>
    public int AliasId { get; set; }

    [ForeignKey(nameof(AliasId))] public EFAlias Alias { get; set; }
}
