namespace GlobalInfraction.WebCore.Shared.Enums;

public enum InfractionType
{
    Warn = 10,
    Mute = 25,
    Kick = 50,
    Unban = 90,
    TempBan = 95,
    Ban = 100
}

public enum InfractionStatus
{
    Revoked = 20,
    Active = 10
}

public enum InfractionScope
{
    Local = 10,
    Global = 20
}
