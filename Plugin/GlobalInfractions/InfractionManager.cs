using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Data.Abstractions;
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
    public readonly ConcurrentDictionary<EFClient, ProfileRequestModel> Profiles = new();

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

    public async Task<ProfileRequestModel> UpdateProfile(EFClient client)
    {
        var httpClient = new HttpClient();

        var profile = new ProfileRequestModel
        {
            ProfileGuid = client.GuidString,
            ProfileGame = client.GameName.ToString(),
            ProfileMeta = new ProfileMetaRequestModel
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            }
        };

        // TODO: This API call isn't even getting to the server. Why?
        /*
         * System.Net.Http.HttpRequestException: An error occurred while sending the request. ---> System.IO.IOException: The response ended prematurely.
         */
        var response = await httpClient.PostAsJsonAsync("http://localhost:5001/api/Profile", profile);
        var profileResult = await response.Content.ReadFromJsonAsync<ProfileRequestModel>();

        if (profileResult is null) throw new Exception("Profile result is null");

        Profiles.TryAdd(client, profileResult);
        Console.WriteLine(profileResult.ProfileGuid);
        return profileResult;
    }

    public async Task NewInfraction(InfractionType infractionType, EFClient origin, EFClient target,
        string reason, string? evidence = null)
    {
        var infraction = new InfractionRequestModel
        {
            InfractionType = infractionType,
            InfractionScope = InfractionScope.Local,
            AdminGuid = origin.GuidString,
            AdminUserName = origin.CleanedName,
            InfractionGuid = Guid.NewGuid(),
            Evidence = evidence,
            Reason = reason,
            Instance = await GetInstance(),
            Profile = Profiles.FirstOrDefault(x => x.Key.GuidString == target.GuidString).Value
        };
        var httpClient = new HttpClient();
        var postResponse = await httpClient.PostAsJsonAsync("http://localhost:5001/api/Infraction", infraction);
        Console.WriteLine($"New infraction: {postResponse.Content.ReadAsStringAsync()}");
    }

    private async Task<InstanceRequestModel> GetInstance()
    {
        var instanceGuid = Plugin.Manager.GetApplicationSettings().Configuration().Id;
        var instanceName = Plugin.Manager.GetApplicationSettings().Configuration().WebfrontCustomBranding;
        var instanceIp = await Utilities.GetExternalIP();
        var apiKey = Guid.Parse("92B3064E-F51D-49F7-9337-C409669B7FDC");

        return new InstanceRequestModel
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
