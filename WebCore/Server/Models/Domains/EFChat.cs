using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanHub.WebCore.Server.Models.Domains;

public class EFChat
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The message sent
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The time the message was sent
    /// </summary>
    public DateTimeOffset Submitted { get; set; }

    /// <summary>
    /// The user who sent the message
    /// </summary>
    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] public EFPlayer Player { get; set; }

    /// <summary>
    /// The server reference this chat message
    /// </summary>
    public int ServerId { get; set; }

    [ForeignKey(nameof(ServerId))] public EFServer Server { get; set; }

    /// <summary>
    /// The community reference this chat message
    /// </summary>
    public int CommunityId { get; set; }

    [ForeignKey(nameof(CommunityId))] public EFCommunity Community { get; set; }
}
