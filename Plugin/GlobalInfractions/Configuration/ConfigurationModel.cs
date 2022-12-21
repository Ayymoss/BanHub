using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Localization;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Configuration;

public class ConfigurationModel : IBaseConfiguration
{
    [JsonPropertyName("ApiKeyDoNotChange")]
    public Guid ApiKey { get; set; } = Guid.NewGuid();

    public string Locale { get; set; } = "EN";
    public Dictionary<string, TranslationStrings> Translations { get; set; } = new() {{"EN", new TranslationStrings()}};

    public string Name() => "GlobalInfractionsSettings";
    public IBaseConfiguration Generate() => new ConfigurationModel();
}

public class TranslationStrings
{
    public string GlobalBanDescription { get; set; } = "Broadcast a global ban to all servers";
}
