using System.Text.Json.Serialization;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions;

public class Configuration : IBaseConfiguration
{
    [JsonPropertyName("ApiKeyDoNotChange")]
    public Guid ApiKey { get; } = Guid.NewGuid();

    public string Name() => "GlobalInfractionsSettings";
    public IBaseConfiguration Generate() => new Configuration();
}
