using System.Collections.Concurrent;
using GlobalInfractions.Configuration;
using GlobalInfractions.Enums;
using GlobalInfractions.Models;
using GlobalInfractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Managers;

public class EndpointManager
{
    public readonly ConcurrentDictionary<EFClient, EntityDto> Profiles = new();


    private readonly ILogger<EndpointManager> _logger;
    private readonly EntityEndpoint _entity;
    private readonly ConfigurationModel _config;
    private readonly InstanceEndpoint _instance;
    private readonly InfractionEndpoint _infraction;

    public EndpointManager(IServiceProvider serviceProvider)
    {
        _entity = new EntityEndpoint(serviceProvider);
        _instance = new InstanceEndpoint(serviceProvider);
        _infraction = new InfractionEndpoint(serviceProvider);
        _logger = serviceProvider.GetRequiredService<ILogger<EndpointManager>>();

        var handler = serviceProvider.GetRequiredService<IConfigurationHandler<ConfigurationModel>>();
        handler.BuildAsync();
        _config = handler.Configuration();
    }

    private static string GetIdentity(string guid, string game) => $"{guid}:{game}";

    public async Task OnJoin(EFClient client, InstanceDto instance)
    {
        var entity = new EntityDto
        {
            Identity = GetIdentity(client.GuidString, client.GameName.ToString()),
            Alias = new AliasDto
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            },
            Instance = instance
        };

        var returnedEntity = await _entity.GetEntity(entity.Identity);

        // Action any penalties
        if (returnedEntity is not null)
        {
            ProcessProfile(returnedEntity, client);
            Profiles.TryAdd(client, returnedEntity);
        }

        // We don't want to act on anything if they're not authenticated
        if (!instance.Active!.Value) return;
        _ = await _entity.UpdateEntity(entity);
    }

    private void ProcessProfile(EntityDto entity, EFClient client)
    {
        var globalBan = entity.Infractions?.FirstOrDefault(x => x is
            {InfractionScope: InfractionScope.Global, InfractionStatus: InfractionStatus.Active});
        if (globalBan is null || !client.IsIngame) return;
        client.Kick(_config.Translations.GlobalBanKickMessage.FormatExt(globalBan.Reason), Utilities.IW4MAdminClient());
    }

    public async Task<bool> UpdateInstance(InstanceDto instance)
    {
        return await _instance.PostInstance(instance);
    }

    public async Task<bool> IsInstanceActive(InstanceDto instance)
    {
        // TODO: Handle this properly if the host is offline for whatever reason. 
        return await _instance.IsInstanceActive(instance.InstanceGuid);
    }

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

    public async Task<bool> NewInfraction(InfractionType infractionType, EFClient origin, EFClient target,
        string reason, TimeSpan? duration = null, InfractionScope? scope = null, string? evidence = null)
    {
        if (!Plugin.Active) return false;

        var targetEntity = Profiles.Any(x => x.Key.GuidString == origin.GuidString);
        var adminEntity = Profiles.Any(x => x.Key.GuidString == target.GuidString);
        
        if (!targetEntity || !adminEntity) return false;

        var infraction = new InfractionDto
        {
            InfractionType = infractionType,
            InfractionScope = scope ?? InfractionScope.Local,
            Evidence = evidence,
            Reason = reason,
            Duration = duration,
            Instance = Plugin.Instance,
            Admin = Profiles.FirstOrDefault(x => x.Key.GuidString == origin.GuidString).Value,
            Target = Profiles.FirstOrDefault(x => x.Key.GuidString == target.GuidString).Value
        };
        return await _infraction.PostInfraction(infraction);
    }
}
