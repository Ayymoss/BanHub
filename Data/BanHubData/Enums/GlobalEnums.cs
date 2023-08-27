namespace BanHubData.Enums;

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
    User = 10,
    Admin = 75,
    SuperAdmin = 100
}

public enum CommunityRole
{
    User = 10,
    Trusted = 20,
    Moderator = 30,
    Administrator = 40,
    SeniorAdmin = 50,
    Owner = 60
}

public enum ModifyPenalty
{
    Revoke,
    Global,
    Local,
    Delete
}
