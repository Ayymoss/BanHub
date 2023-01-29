﻿namespace BanHub.Enums;

public enum PenaltyType
{
    Warn = 10,
    Mute = 25,
    Kick = 50,
    Unban = 90,
    TempBan = 95,
    Ban = 100
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

public enum WebRole
{
    User = 10,
    Admin = 75,
    SuperAdmin = 100
}
