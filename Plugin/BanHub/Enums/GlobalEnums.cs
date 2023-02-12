namespace BanHub.Enums;

public enum PenaltyType
{
    Report,
    Warning,
    Mute,
    TempMute,
    Unmute,
    Kick,
    Unban,
    TempBan,
    Ban
}

public enum PenaltyStatus
{
    Information = 0,
    Active = 10,
    Revoked = 20,
    Expired = 30
}

public enum PenaltyScope
{
    Local = 10,
    Global = 20
}

public enum Action
{
    Add,
    Remove
}

public enum InstanceRole
{
    InstanceUser = 10,
    InstanceTrusted = 20,
    InstanceModerator = 30,
    InstanceAdministrator = 40,
    InstanceSeniorAdmin = 50,
    InstanceOwner = 60
}
