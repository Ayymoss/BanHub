using BanHub.Domain.Enums;

namespace BanHub.Domain.ValueObjects;

public class CreateOrUpdatePlayerNotificationSlim
{
    public string PlayerIdentity { get; set; }
    public string PlayerAliasUserName { get; set; }
    public string PlayerAliasIpAddress { get; set; }
    public CommunityRole PlayerCommunityRole { get; set; }
    public Guid CommunityGuid { get; set; }
    public string? ServerId { get; set; }
}
