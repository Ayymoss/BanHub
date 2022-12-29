﻿namespace GlobalInfraction.WebCore.Server.Services;

public class Configuration
{
    public byte Version { get; set; } = 1;
    public int WebBind { get; set; } = 80;
    public string? DiscordHook { get; set; }
    public DatabaseType DatabaseType { get; set; } = DatabaseType.Sqlite;
    public DatabaseConfiguration Database { get; set; } = new();
}

public class DatabaseConfiguration
{
    public string HostName { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Database { get; set; }
    
}
public enum DatabaseType
{
    Postgres,
    Sqlite
}