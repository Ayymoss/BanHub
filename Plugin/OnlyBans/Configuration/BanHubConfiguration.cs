using System.Text.Json.Serialization;
using SharedLibraryCore.Interfaces;

namespace BanHub.Configuration;

public class BanHubConfiguration
{
    [JsonIgnore] public const string Name = "Ban Hub";
    public bool EnableBanHub { get; set; } = true;
    [JsonPropertyName("ApiKeyDoNotChange")] public Guid ApiKey { get; set; } = Guid.NewGuid();
    public string? InstanceNameOverride { get; set; }
    public bool PrintPenaltyToConsole { get; set; } = false;
    public TranslationStrings Translations { get; set; } = new();
    public bool DebugMode { get; set; } = false;
}

public class TranslationStrings
{
    // @formatter:off
    // ReSharper disable once InconsistentNaming
    public string CannotAuthIW4MAdmin { get; set; } = "You cannot authenticate as IW4MAdmin";
    public string NotActive { get; set; } = "Ban Hub is not active";
    public string GlobalBanCommandSuccess { get; set; } = "Ban Hub banned {{target}} for {{reason}} ({{guid}})";
    public string GlobalBanCommandSuccessFollow { get; set; } = "(Color::Yellow)You must upload (Color::Accent)!evidence (Color::Yellow)for global bans!";
    public string GlobalBanCommandFail { get; set; } = "Ban Hub ban was not submitted";
    public string SubmitEvidenceSuccess { get; set; } = "Evidence submitted";
    public string SubmitEvidenceFail { get; set; } = "Failed to submit evidence. Does the penalty exist or have evidence already?";
    public string SubmitEvidenceRegexFail { get; set; } = "Evidence must be a valid YouTube URL";
    public string CannotTargetServer { get; set; } = "You cannot ban the console...";
    public string ProvideToken { get; set; } = "Your token is {{token}} (expires in 5 minutes)";
    public string TokenGenerationFailed { get; set; } = "Failed to generate token";
    // @formatter:on
}
