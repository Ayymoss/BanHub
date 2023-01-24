using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalInfraction.WebCore.Server.Models;

public class EFAuthToken
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The token
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// When the token was created
    /// </summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>
    /// If the token has been used
    /// </summary>
    public bool Used { get; set; }

    /// <summary>
    /// Entity associated with current token.
    /// </summary>
    public int EntityId { get; set; }

    [ForeignKey(nameof(EntityId))] public EFEntity Entity { get; set; } = null!;
}
