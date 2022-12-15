using System.ComponentModel.DataAnnotations;

namespace GlobalInfraction.WebCore.Server.Models;

public class EFProfile
{
    [Key] public int Id { get; set; }

    /// <summary>
    /// The player's in-game GUID
    /// </summary>
    public string ProfileGuid { get; init; } = null!;

    /// <summary>
    /// The player's game tied GUID
    /// </summary>
    public string ProfileGame { get; init; } = null!;

    /// <summary>
    /// Unique profile identifier for comparing profiles
    /// </summary>
    public string ProfileIdentifier { get; init; } = null!;

    /// <summary>
    /// The player's reputation
    /// </summary>
    public int Reputation { get; set; } = 10;

    /// <summary>
    /// The player's last connected date
    /// </summary>
    public DateTimeOffset LastConnected { get; set; }

    /// <summary>
    /// The player's list of names and IP addresses
    /// </summary>
    public virtual ICollection<EFProfileMeta> ProfileMetas { get; set; } = null!;

    /// <summary>
    /// The player's list of infractions
    /// </summary>
    public virtual ICollection<EFInfraction> Infractions { get; set; } = null!;
}

// TODO:
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
    
    Removing a ban needs to check instance ID
    
    Onboarding process - Server owners download, install then the plugin makes a request to me for acceptance. 
*/


/* // Unedited https://forums.mcbans.com/wiki/how-the-system-works/
Types of Bans

Global and Local Bans:

A “global” ban is one that is visible to other servers and affects a player’s reputation score. Global bans can only be issued for one of the reasons outlined in the list of Global Ban Rules, otherwise they will be switched to local.

A “local” ban is one that does not affect a player’s reputation score, and is not visible to the public via the MCBans website. Servers can issue local bans on any player they like for whatever reason they like. This way, servers have complete control over who can and cannot connect to their server, but cannot damage a player’s reputation to other servers without a valid reason.

If a global ban is disputed and the server responsible is unable to provide valid proof, the global ban will be switched to local.

Other Bans:

“Permabans” are issued directly by the MCBans staff. These bans will drop a player’s reputation score immediately to 0, and are issued for security purposes when an account is compromised, or when a user is intentionally using the system for malicious purposes.

“Tempbans” expire after a specified amount of time and can be used by servers for management purposes. They do not affect the reputation system and are intended as a tool for server staff to use when something more effective than a kick but less drastic than a ban is needed.

IP bans do not affect the reputation system and can be used to protect a server when a player has access to multiple accounts.


Reputation Points

Players and servers each have a reputation score that is visible on MCBans.com. Players have a default score of 10, and servers have a default score of 1, but can have a maximum of 3 if earned through good behavior.
When a player is globally banned from a server, the reputation of the server is subtracted from the reputation of the player. For example, a player banned from a server with a reputation of 2 will have a reputation of 8. Please note that only global bans affect a player’s reputation. Local bans are not taken into account.

Each server specifies the minimum reputation a player must have in order to connect.

Servers can gain reputation points by uploading valid proof and winning ban disputes. Servers will lose reputation for issuing invalid bans, being unable to provide proof for a dispute, or by going inactive on the system. If a server’s reputation is reduced to 0, the server is removed from the system and all bans made void.


Appeals and Disputes

When a player feels that a global ban is unfair, they can dispute the ban through MCBans.com. During the appeal stage, the player will have a chance to provide their side of the story and communicate with the server staff to see if an agreement can be reached. This can result in the server removing the ban of their own accord.

If an agreement cannot be reached after a week in the appeal process, the appeal will escalate to a “dispute,” at which point an MCBans staff member will review the discussion. The server will be required to provide valid proof for the ban within a week, or the ban will be switched to local, meaning it no longer affects the player’s reputation.

Servers will receive a penalty to their reputation score if they are unable to provide valid proof to win a dispute.

Both parties are able to escalate an appeal to a dispute at any time if they do not want to wait the entire week, but are strongly encouraged only to do so after both sides have had a chance to respond.


MCBans for Servers

As a server on the MCBans system, you have a variety of options in terms of how you would like to make use of the global ban list.

Each server can specify their reputation threshold in your MCBans config file. This number determines the minimum reputation a player must have in order to connect to your server. That way, you can control how strict you would like to be in allowing people to connect.

For example, if you would like to play it extremely safe, you can set your reputation threshold to 10, meaning that only players with a perfect record can connect. If you were to set your reputation threshold to 5, players who have lost a significant amount of reputation from multiple global bans would be blocked from connecting, but players with only 1 or 2 would still be able to play.

If you set your threshold to 0, all players will be able to connect except for permabanned ones.

You can also set your reputation to -1, meaning that any player can connect, including permabanned ones. This is useful if you would like to use MCBans to view each player’s ban history in-game, but do not want the MCbans plugin to stop players from logging on based on their reputation score.

Servers can also use the “groups” function to only trust bans from a specific set of servers, or to exclude information from servers that the user deems as untrustworthy, or use the “player exceptions” list on the server dashboard to make exceptions for specific players.


MCBans for Players

As a player, you have the option of reviewing your ban history and challenging those that you think were given wrongfully.

If the ban is issued by a server, you can do this by viewing your bans under the “my account” tab on the MCBans.com site, and clicking “dispute ban” next to the ban you think is unfair.

Please note that regardless of your reputation score, you will be able to connect to some MCBans servers depending on how they have configured their reputation threshold.

In rare cases, your ban may be a permaban, in which case there is an appeal section on the forums where you’ll be able to discuss the matter with a staff member.


MCBans Staff

The MCBans staff serve several purposes. All staff members monitor the global ban list and will switch bans issued for invalid reasons to local. MCBans staff may occasionally visit servers to alert the staff that invalid bans have been issued.

The MCBans dispute staff will review ban disputes and check to make sure that any proof provided is valid.

The MCBans support staff are available on the forums to answer questions related to the plugin, technical and otherwise.

MCBans staff do not issue bans from MCBans servers unless they are staff members on the server in question. They are able to issue permabans from MCBans.com, but only when an account is compromised or when server staff are using the system maliciously.

*/
