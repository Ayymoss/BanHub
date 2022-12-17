using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
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
    public readonly ConcurrentDictionary<EFClient, ProfileDto> Profiles = new();

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

    public static string GetIdentity(string guid, string game) => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{guid}:{game}"));

    public async Task UpdateProfile(EFClient client)
    {
        var httpClient = new HttpClient();

        var profile = new ProfileDto
        {
            ProfileIdentity = GetIdentity(client.GuidString, client.GameName.ToString()),
            ProfileMeta = new ProfileMetaDto
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            },
            Instance = await GetInstance()
        };

        var response = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Profile", profile);
        var profileResult = await response.Content.ReadFromJsonAsync<ProfileDto>();

        if (profileResult is null) throw new Exception("Profile result is null");

        Profiles.TryAdd(client, profileResult);
        ProcessProfile(profileResult, client);
    }

    public void ProcessProfile(ProfileDto profile, EFClient client)
    {
        var globalBan = profile.Infractions?.FirstOrDefault(x => x is
            {InfractionScope: InfractionScope.Global, InfractionStatus: InfractionStatus.Active});
        if (globalBan is null || !client.IsIngame) return;
        var activePenalty = client.ReceivedPenalties.FirstOrDefault(x => x.Active && x.Type == EFPenalty.PenaltyType.Ban);
        if (activePenalty is not null) return;
        client.Kick($"GLOBAL: {globalBan.Reason}", Utilities.IW4MAdminClient());
    }
    // TODO: Fix recursive call. Global ban from another community will trigger this and then upload a new infraction.
    
    public async Task NewInfraction(InfractionType infractionType, EFClient origin, EFClient target,
        string reason, TimeSpan? duration = null, string? evidence = null)
    {
        if (!Plugin.Active) return;
        
        var infraction = new InfractionDto
        {
            InfractionType = infractionType,
            InfractionScope = InfractionScope.Local,
            InfractionGuid = Guid.NewGuid(),
            Evidence = evidence,
            Reason = reason,
            Duration = duration,
            Instance = await GetInstance(),
            Admin = Profiles.FirstOrDefault(x => x.Key.GuidString == origin.GuidString).Value,
            Target = Profiles.FirstOrDefault(x => x.Key.GuidString == target.GuidString).Value
        };
        var httpClient = new HttpClient();
        var postResponse = await httpClient.PostAsJsonAsync("http://localhost:5000/api/Infraction", infraction);
        Console.WriteLine($"New infraction: {postResponse.Content.ReadAsStringAsync()}");
    }

    private async Task<InstanceDto> GetInstance()
    {
        var instanceGuid = Plugin.Manager.GetApplicationSettings().Configuration().Id;
        var instanceName = Plugin.Manager.GetApplicationSettings().Configuration().WebfrontCustomBranding;
        var instanceIp = await Utilities.GetExternalIP();
        var apiKey = Plugin.Configuration.ApiKey; 

        return new InstanceDto
        {
            InstanceGuid = Guid.Parse(instanceGuid),
            InstanceName = instanceName,
            InstanceIp = instanceIp,
            ApiKey = apiKey
        };
    }

    public async Task<bool> UpdateInstance()
    {
        var instance = await GetInstance();

        var httpClient = new HttpClient();
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

        if (!enabled)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Global Bans plugin is disabled");
            Console.WriteLine($"[{Plugin.PluginName}] To activate your access. Please visit <DISCORD>");
        }
        else
        {
            Console.WriteLine($"[{Plugin.PluginName}] Global Bans plugin is enabled");
            Console.WriteLine($"[{Plugin.PluginName}] Infractions will be reported to the Global Bans list.");
        }

        return enabled;
    }
}
