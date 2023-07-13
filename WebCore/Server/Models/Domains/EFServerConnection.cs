using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Domains;

public class EFServerConnection
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The time they connected
    /// </summary>
    public required DateTimeOffset Connected { get; set; }

    /// <summary>
    /// Entity associated with the connection
    /// </summary>
    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; } = null!;

    /// <summary>
    /// The server they connected to
    /// </summary>
    public int ServerId { get; set; }

    [ForeignKey(nameof(ServerId))] public EFServer Server { get; set; } = null!;
}
