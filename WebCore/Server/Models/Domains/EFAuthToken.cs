using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Domains;

public class EFAuthToken
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The token
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// When the token was created
    /// </summary>
    public required DateTimeOffset Created { get; set; }

    /// <summary>
    /// If the token has been used
    /// </summary>
    public required bool Used { get; set; }

    /// <summary>
    /// Entity associated with current token.
    /// </summary>
    public int EntityId { get; set; }

    [ForeignKey(nameof(EntityId))] public EFPlayer Player { get; set; } = null!;
}
