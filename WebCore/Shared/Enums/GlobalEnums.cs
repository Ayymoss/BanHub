namespace GlobalBan.WebCore.Shared.Enums;

public enum InfractionType
{
    Warn = 10,
    Mute = 25,
    Kick = 50,
    Ban = 100
}

public enum InfractionStatus
{
    Inactive = 20,
    Active = 10
}

public enum InfractionScope
{
    Local = 10,
    Global = 20
}
