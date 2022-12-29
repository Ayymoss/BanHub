using System.Collections.Concurrent;
using GlobalInfractions.Configuration;
using GlobalInfractions.Enums;
using GlobalInfractions.Models;
using GlobalInfractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;

namespace GlobalInfractions.Managers;

public class EndpointManager
{
    public readonly ConcurrentDictionary<EFClient, EntityDto> Profiles = new();

    private readonly ILogger<EndpointManager> _logger;
    private readonly EntityEndpoint _entity;
    private readonly ConfigurationModel _configurationModel;
    private readonly InstanceEndpoint _instance;
    private readonly InfractionEndpoint _infraction;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public EndpointManager(IServiceProvider serviceProvider, ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _entity = new EntityEndpoint(configurationModel);
        _instance = new InstanceEndpoint(configurationModel);
        _infraction = new InfractionEndpoint(configurationModel);
        _logger = serviceProvider.GetRequiredService<ILogger<EndpointManager>>();
    }

    private static string GetIdentity(EFClient client) => $"{client.GuidString}:{client.GameName.ToString()}";

    private static EntityDto ClientToEntity(EFClient client)
    {
        return new EntityDto
        {
            Identity = GetIdentity(client),
            Alias = new AliasDto
            {
                UserName = client.CleanedName,
                IpAddress = client.IPAddressString
            },
            Instance = Plugin.Instance
        };
    }

    public async Task OnJoin(EFClient client)
    {
        var entity = ClientToEntity(client);

        var returnedEntity = await _entity.GetEntity(entity.Identity);

        // Action any penalties
        if (returnedEntity is not null)
        {
            if (Plugin.FeaturesEnabled) ProcessEntity(returnedEntity, client); // TODO: Remove tag
            Profiles.TryAdd(client, returnedEntity);
        }

        // We don't want to act on anything if they're not authenticated
        if (!Plugin.Instance.Active!.Value) return;

        var entityUpdated = await _entity.UpdateEntity(entity);

        // If they're new we need to add them to the profiles
        if (returnedEntity is null && entityUpdated)
        {
            var newEntity = await _entity.GetEntity(entity.Identity);
            if (newEntity is not null)
            {
                if (Plugin.FeaturesEnabled) ProcessEntity(newEntity, client); // TODO: Remove tag
                Profiles.TryAdd(client, newEntity);
            }
        }
    }

    private void ProcessEntity(EntityDto entity, EFClient client)
    {
        var globalBan = entity.Infractions?
            .FirstOrDefault(x => x.InfractionType is InfractionType.Ban && x.InfractionScope is InfractionScope.Global);
        if (globalBan is null || !client.IsIngame) return;
        client.Kick(_configurationModel.Translations.GlobalBanKickMessage
            .FormatExt(globalBan.Reason), Utilities.IW4MAdminClient(client.CurrentServer));
    }

    public async Task<bool> UpdateInstance(InstanceDto instance)
    {
        return await _instance.PostInstance(instance);
    }

    public async Task<bool> IsInstanceActive(InstanceDto instance)
    {
        // TODO: Handle this properly if the API is offline for whatever reason. 
        return await _instance.IsInstanceActive(instance.InstanceGuid);
    }

    public async void RemoveFromProfiles(EFClient client)
    {
        try
        {
            await _semaphore.WaitAsync();

            var remove = Profiles.TryRemove(client, out _);
            if (remove)
            {
                _logger.LogInformation("Removed {Name} from profiles", client.CleanedName);
                return;
            }

            _logger.LogError("Failed to remove {Name} from profiles", client.CleanedName);
        }
        finally
        {
            if (_semaphore.CurrentCount == 0) _semaphore.Release();
        }
    }

    public async Task<bool> NewInfraction(InfractionType infractionType, EFClient origin, EFClient target,
        string reason, TimeSpan? duration = null, InfractionScope? scope = null, string? evidence = null)
    {
        if (!Plugin.InstanceActive) return false;

        try
        {
            await _semaphore.WaitAsync();
            
            var adminEntity = ClientToEntity(origin);
            var targetEntity = ClientToEntity(target);

            var infraction = new InfractionDto
            {
                InfractionType = infractionType,
                InfractionScope = scope ?? InfractionScope.Local,
                Evidence = evidence,
                Reason = reason,
                Duration = duration,
                Instance = Plugin.Instance,
                Admin = adminEntity,
                Target = targetEntity
            };
            return await _infraction.PostInfraction(infraction);
        }
        finally
        {
            if (_semaphore.CurrentCount == 0) _semaphore.Release();
        }
    }
}
