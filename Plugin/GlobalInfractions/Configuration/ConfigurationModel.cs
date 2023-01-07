using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Localization;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Configuration;

public class ConfigurationModel : IBaseConfiguration
{
    public bool PluginEnabled { get; set; } = true;
    [JsonPropertyName("ApiKeyDoNotChange")]
    public Guid ApiKey { get; set; } = Guid.NewGuid();
    public string? InstanceNameOverride { get; set; }
    public bool PrintInfractionsToConsole { get; set; } = false;
    public TranslationStrings Translations { get; set; } = new();
    public List<int> WhitelistedClientIds { get; set; } = new();

    public string Name() => "GlobalInfractionsSettings";
    public IBaseConfiguration Generate() => new ConfigurationModel();
}

public class TranslationStrings
{
    public string GlobalBanKickMessage { get; set; } = "(Color::Red)Globally banned!(Color::White)\nGlobalInfractions.com";
    public string NotActive { get; set; } = "Global Infractions is not active";
    public string GlobalBanCommandSuccess { get; set; } = "Global Infractions banned {{target}} for {{reason}} ({{guid}})";
    public string GlobalBanCommandFail { get; set; } = "Global Infractions ban was not submitted";
    public string SubmitEvidenceSuccess { get; set; } = "Evidence submitted";
    public string SubmitEvidenceFail { get; set; } = "Failed to submit evidence";
    public string GlobalBanCommandSuccessFollow { get; set; } = "(Color::Yellow)You must upload (Color::Accent)!evidence (Color::Yellow)for global bans!";
    public string CannotTargetServer { get; set; } = "You cannot ban the console...";
}
