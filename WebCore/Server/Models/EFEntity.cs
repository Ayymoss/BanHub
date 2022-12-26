using System.ComponentModel.DataAnnotations;
using GlobalInfraction.WebCore.Shared.Enums;

namespace GlobalInfraction.WebCore.Server.Models;

/// <summary>
/// Table for all players
/// </summary>
public class EFEntity
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The player's identity (Format: GAMEGUID:GAMEID)
    /// </summary>
    public string Identity { get; init; } = null!;

    /// <summary>
    /// The player's reputation
    /// </summary>
    public int Reputation { get; set; } = 10;

    /// <summary>
    /// The last time the player was seen
    /// </summary>
    public DateTimeOffset HeartBeat { get; set; }

    /// <summary>
    /// The player's web-login role
    /// </summary>
    public WebRole WebRole { get; set; }

    /// <summary>
    /// The player's list of names and IP addresses
    /// </summary>
    public virtual ICollection<EFAlias> Aliases { get; set; } = null!;

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<EFInfraction> Infractions { get; set; } = null!;

    /// <summary>
    /// The lookup for current alias
    /// </summary>
    public virtual EFCurrentAlias CurrentAlias { get; set; } = null!;
}

/*
Defaults:
    Players = 10
    Servers = 1
        Max = 3
        Reputation increases if global ban is provided with proof OR wins a dispute.
        Reputation decreases if global ban is not provided with proof OR loses a dispute.
        
    Servers have control over minimum reputation required to join
    Global bans affect reputation
    Local bans do not affect reputation
    
    Servers offline for more than 1 day will decay in reputation; eventually removed from the list at 0.
        
    Onboarding process - Server owners download, install then the plugin makes a request to me for acceptance. 
    
    TODO: REPUTATION DECAY -- Not important
    TODO: RECURSIVE CALL ISSUE
    TODO: PRIVILEGED USERS -- Half done
    TODO: CHANGE TO POSTGRES
    TODO: FRONTEND
    TODO: Handle HTTP server disconnects better. We should retry connection if it fails. We should also queue infractions. 
    TODO: REMOVE DISCONNECTS FROM PROFILES (Plugin)
    TODO: CHECK FOR NOTHING IN PROFILES (Plugin)
    
    TODO: USE AUTOMAPPER FOR OBJECT MAPPING
    
    TODO: Add web-authentication. -- Not important
    
    Possible implementations:
    TODO: Do I want to track the list of connected instances?
    
*/
// Unedited https://forums.mcbans.com/wiki/how-the-system-works/
