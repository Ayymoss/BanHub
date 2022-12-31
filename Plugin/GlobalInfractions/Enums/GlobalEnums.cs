namespace GlobalInfractions.Enums;

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
    Information = 0,
    Active = 10,
    Revoked = 20,
    Expired = 30
}

public enum InfractionScope
{
    Local = 10,
    Global = 20
}

public enum Action
{
    Add,
    Remove
}
