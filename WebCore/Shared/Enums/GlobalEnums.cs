namespace BanHub.WebCore.Shared.Enums;

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

public enum Game
{
    COD = -1,
    UKN = 0,
    IW3 = 1,
    IW4 = 2,
    IW5 = 3,
    IW6 = 4,
    T4 = 5,
    T5 = 6,
    T6 = 7,
    T7 = 8,
    SHG1 = 9,
    CSGO = 10,
    H1 = 11
}

public enum PenaltyStatus
{
    Informational = 0,
    Active = 10,
    Revoked = 20,
    Expired = 30
}

public enum PenaltyScope
{
    Local = 10,
    Global = 20
}

public enum WebRole
{
    WebUser = 10,
    WebAdmin = 75,
    WebSuperAdmin = 100
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
