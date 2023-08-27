using System.Text.Json.Serialization;

namespace BanHub.Configuration;

public class BanHubConfiguration
{
    [JsonIgnore] public const string Name = "Ban Hub";
    public bool EnableBanHub { get; set; } = true;
    public bool BroadcastGlobalBans { get; set; } = true;
    public bool PrintPenaltiesToConsole { get; set; } = false;
    public bool NotifyOnlyMode { get; set; } = false;
    public string? CommunityNameOverride { get; set; }
    public string? CommunityWebsite { get; set; }
    public TranslationStrings Translations { get; set; } = new();
    [JsonPropertyName("ApiKeyDoNotChange")] public Guid ApiKey { get; set; } = Guid.NewGuid();
}

public class TranslationStrings
{
    // @formatter:off
    public string BanHubName { get; set; } = "[(Color::Red)BanHub(Color::White)]";
    // ReSharper disable once InconsistentNaming
    public string CannotAuthIW4MAdmin { get; set; } = "{{plugin}} You cannot authenticate as IW4MAdmin";
    public string NotActive { get; set; } = "{{plugin}} is not active";
    public string GlobalBanCommandSuccess { get; set; } = "{{plugin}} Banned {{target}} for {{reason}} ({{guid}})";
    public string GlobalBanCommandSuccessFollow { get; set; } = "{{plugin}} (Color::Yellow)You must upload evidence (Color::Accent)!{{command}} (Color::Yellow)for global bans!";
    public string GlobalBanCommandFail { get; set; } = "{{plugin}} Ban Hub ban was not submitted";
    public string SubmitEvidenceSuccess { get; set; } = "{{plugin}} Evidence submitted";
    public string SubmitEvidenceFail { get; set; } = "{{plugin}} Failed to submit evidence. Does the penalty exist or have evidence already?";
    public string SubmitEvidenceUrlFail { get; set; } = "{{plugin}} Evidence must be a valid YouTube URL";
    public string CannotTargetServer { get; set; } = "{{plugin}} You cannot ban the console...";
    public string ProvideToken { get; set; } = "{{plugin}} Your token is {{token}} (expires in 5 minutes)";
    public string TokenGenerationFailed { get; set; } = "{{plugin}} Failed to generate token";
    public string UserHasNotes { get; set; } = "{{plugin}} (Color::Accent){{user}} (Color::White)joined and has (Color::Yellow){{count}} (Color::White)note(s)";
    public string UserIsBanned { get; set; } = "{{plugin}} (Color::Accent){{user}} (Color::White)joined and is (Color::Red)Global Banned(Color::White)";
    public string BroadcastGlobalBan { get; set; } = "{{plugin}} (Color::Accent){{user}} (Color::Grey)({{identity}}) (Color::Red)Global Banned";
    // @formatter:on
}
