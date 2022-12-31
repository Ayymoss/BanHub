using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Localization;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Configuration;

public class ConfigurationModel : IBaseConfiguration
{
    [JsonPropertyName("ApiKeyDoNotChange")]
    public Guid ApiKey { get; set; } = Guid.NewGuid();

    public string? InstanceNameOverride { get; set; }

    public TranslationStrings Translations { get; set; } = new();

    public string Name() => "GlobalInfractionsSettings";
    public IBaseConfiguration Generate() => new ConfigurationModel();
}

public class TranslationStrings
{
    public string GlobalBanKickMessage { get; set; } = "GLOBALLY BANNED: {{reason}}";
    public string NotActive { get; set; } = "Global Infractions is not active";
    public string GlobalBanCommandSuccess { get; set; } = "Global Infractions banned {{target}} for {{reason}}";
    public string GlobalBanCommandFail { get; set; } = "Global Infractions ban was not submitted";
}
