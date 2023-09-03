using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Models.Domains;

public class EFServer
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The server Id
    /// </summary>
    public required string ServerId { get; set; }

    /// <summary>
    /// The server name
    /// </summary>
    public required string ServerName { get; set; }

    /// <summary>
    /// The server IP
    /// </summary>
    public required string ServerIp { get; set; }

    /// <summary>
    /// The port of the server
    /// </summary>
    public required int ServerPort { get; set; }

    /// <summary>
    /// The game the server is running
    /// </summary>
    public required Game ServerGame { get; set; }

    /// <summary>
    /// When we last received an update from this server.
    /// </summary>
    public required DateTimeOffset Updated { get; set; }

    /// <summary>
    /// The maximum number of players allowed on the server.
    /// </summary>
    public int MaxPlayers { get; set; } = 0; // TODO: Remove this for live

    /// <summary>
    /// Current server associated with instance.
    /// </summary>
    public int CommunityId { get; set; }

    [ForeignKey(nameof(CommunityId))] public EFCommunity Community { get; set; }
}
