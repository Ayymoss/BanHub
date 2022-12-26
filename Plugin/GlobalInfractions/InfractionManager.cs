using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Data.Abstractions;
using Data.Models;
using GlobalInfractions.Enums;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions;

public class InfractionManager
{
    public readonly ConcurrentDictionary<EFClient, EntityDto> Profiles = new();
    public InstanceDto Instance = null!;

    private readonly IMetaServiceV2 _metaService;
    private readonly ITranslationLookup _translationLookup;
    private readonly ILogger<InfractionManager> _logger;
    private readonly IDatabaseContextFactory _context;
    

    public InfractionManager(IServiceProvider serviceProvider)
    {
        _metaService = serviceProvider.GetRequiredService<IMetaServiceV2>();
        _translationLookup = serviceProvider.GetRequiredService<ITranslationLookup>();
        _logger = serviceProvider.GetRequiredService<ILogger<InfractionManager>>();
        _context = serviceProvider.GetRequiredService<IDatabaseContextFactory>();
    }

    public static string GetIdentity(string guid, string game) => $"{guid}:{game}";

    public void RemoveFromProfiles(EFClient client)
    {
        var remove = Profiles.TryRemove(client, out _);
        if (remove)
        {
            _logger.LogInformation("Removed {Name} from profiles", client.CleanedName);
            return;
        }

        _logger.LogError("Failed to remove {Name} from profiles", client.CleanedName);
    }

    public async Task UpdateProfile(EFClient client)
    {

        Console.WriteLine("Updating profile for {0}", client.CleanedName);
        var httpClient = new HttpClient();

        var profile = new EntityDto
        {
            ProfileIdentity = GetIdentity(client.GuidString, client.GameName.ToString()),
            Alias = new AliasDto
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            },
            Instance = Instance
        };

        Console.WriteLine($"Request for {client.CleanedName}: {JsonSerializer.Serialize(profile)}");
        var response = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Profile", profile);
        Console.WriteLine($"Response for {client.CleanedName}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        
        if (response.StatusCode is not HttpStatusCode.OK) return;
        
        var profileResult = await response.Content.ReadFromJsonAsync<EntityDto>();
        
        if (profileResult is null)
        {
            _logger.LogWarning("Failed to get profile from API. Are we active? {StatusCode}", response.StatusCode);
            return;
        }

        Console.WriteLine("Trying to add {0}", profileResult.Alias.UserName);
        Profiles.TryAdd(client, profileResult);
        // TODO: This needs to be called on user even if not active
        ProcessProfile(profileResult, client);
        Console.WriteLine("Added profile {0}", client.CleanedName);
    }

    public void ProcessProfile(EntityDto profile, EFClient client)
    {
        var globalBan = profile.Infractions?.FirstOrDefault(x => x is
            {InfractionScope: InfractionScope.Global, InfractionStatus: InfractionStatus.Active});
        if (globalBan is null || !client.IsIngame) return;
        var activePenalty = client.ReceivedPenalties.FirstOrDefault(x => x.Active && x.Type == EFPenalty.PenaltyType.Ban);
        if (activePenalty is not null) return;
        client.Kick($"GLOBAL: {globalBan.Reason}", Utilities.IW4MAdminClient());
    }
    // TODO: Fix recursive call. Global ban from another community will trigger this and then upload a new infraction.

    public async Task<string> NewInfraction(InfractionType infractionType, EFClient origin, EFClient target,
        string reason, TimeSpan? duration = null, InfractionScope? scope = null, string? evidence = null)
    {
        if (!Plugin.Active) return "Plugin is not active";

        var infraction = new InfractionDto
        {
            InfractionType = infractionType,
            InfractionScope = scope ?? InfractionScope.Local,
            Evidence = evidence,
            Reason = reason,
            Duration = duration,
            Instance = Instance,
            Admin = Profiles.FirstOrDefault(x => x.Key.GuidString == origin.GuidString).Value,
            Target = Profiles.FirstOrDefault(x => x.Key.GuidString == target.GuidString).Value
        };
        var httpClient = new HttpClient();
        var postResponse = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Infraction", infraction);

        Console.WriteLine($"STATUS: {postResponse.StatusCode} - JSON: {JsonSerializer.Serialize(infraction)} ");
        return $"New infraction: {await postResponse.Content.ReadAsStringAsync()}";
    }

    public async Task GetInstance()
    {
        var instanceGuid = Plugin.Manager.GetApplicationSettings().Configuration().Id;
        var instanceName = Plugin.Manager.GetApplicationSettings().Configuration().WebfrontCustomBranding;
        var instanceIp = await Utilities.GetExternalIP();
        var apiKey = Plugin.Configuration.ApiKey;

        Instance =  new InstanceDto
        {
            InstanceGuid = Guid.Parse(instanceGuid),
            InstanceName = instanceName,
            InstanceIp = instanceIp,
            ApiKey = apiKey,
            Heartbeat = DateTimeOffset.UtcNow
        };
    }

    public async Task<bool> UpdateInstance()
    {
        var instance = Instance;

        var httpClient = new HttpClient();
        // TODO: Handle this properly if the host is offline for whatever reason. 
        // it should retry.
        var postServerResponse = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Instance", instance);
        var enabled = false;

        switch (postServerResponse.StatusCode)
        {
            case HttpStatusCode.Accepted:
                enabled = true;
                break;
            case HttpStatusCode.OK:
                enabled = false;
                break;
        }

        return enabled;
    }
}
