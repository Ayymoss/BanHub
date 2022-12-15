using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GlobalBan.WebCore.Shared.Enums;

namespace GlobalBan.WebCore.Server.Models;

public class EFInfraction
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The type of infraction
    /// </summary>
    public InfractionType InfractionType { get; set; }

    /// <summary>
    /// The state of the infraction
    /// </summary>
    public InfractionStatus InfractionStatus { get; set; }

    /// <summary>
    /// The scope of the infraction
    /// </summary>
    public InfractionScope InfractionScope { get; set; }

    /// <summary>
    /// The unique infraction identifier
    /// </summary>
    public Guid InfractionGuid { get; set; }

    /// <summary>
    /// Time of the infraction
    /// </summary>
    public DateTimeOffset Submitted { get; set; }

    /// <summary>
    /// The admin GUID who issued the infraction
    /// </summary>
    public string AdminGuid { get; set; } = null!;

    /// <summary>
    ///  The admin who issued the infraction
    /// </summary>
    public string AdminUserName { get; set; } = null!;

    /// <summary>
    /// The provided reason for the infraction
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// The uploaded evidence for the infraction
    /// </summary>
    public string? Evidence { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))] public EFProfile Profile { get; set; } = null!;
    public int ServerId { get; set; }
    [ForeignKey(nameof(ServerId))] public EFInstance Instance { get; set; } = null!;
}
