﻿namespace GlobalInfraction.WebCore.Shared.Enums;

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
    Active = 10,
    Revoked = 20
    
}

public enum InfractionScope
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
