using System.Collections.Concurrent;
using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.SignalR;
using BanHub.Domain.ValueObjects.Plugin.SignalR;
using BanHub.Infrastructure.Context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.SignalR.Hubs;

public class TomatoCounterHub(DataContext context) : Hub, ITomatoHub
{
    private static readonly ConcurrentDictionary<string, int> TomatoCounters = new();

    private readonly SemaphoreSlim _tomatoLock = new(1, 1);

    public async Task<int> GetTomatoCount(string identity)
    {
        if (!TomatoCounters.ContainsKey(identity)) await LoadTomatoes(identity);
        await Groups.AddToGroupAsync(Context.ConnectionId, identity);
        return TomatoCounters.GetValueOrDefault(identity, 0);
    }

    public async Task IncrementTomatoCount(string identity)
    {
        try
        {
            await _tomatoLock.WaitAsync();

            if (!TomatoCounters.TryGetValue(identity, out var count)) await LoadTomatoes(identity);

            count++;
            TomatoCounters[identity] = count;

            await Clients.Group(identity).SendAsync(SignalRMethods.TomatoMethods.OnTomatoCountUpdate, count);
            if (count % 123 is 0) await StoreTomatoes(identity);
        }
        finally
        {
            if (_tomatoLock.CurrentCount is 0) _tomatoLock.Release();
        }
    }

    private async Task LoadTomatoes(string identity)
    {
        var tomatoCounter = await context.TomatoCounters
            .AsNoTracking()
            .Where(x => x.Player.Identity == identity)
            .FirstOrDefaultAsync();

        if (tomatoCounter is not null) TomatoCounters[identity] = tomatoCounter.Tomatoes;
    }

    private async Task StoreTomatoes(string identity)
    {
        if (!TomatoCounters.TryGetValue(identity, out var count)) return;

        var tomatoCounter = await context.TomatoCounters
            .Where(x => x.Player.Identity == identity)
            .FirstOrDefaultAsync();

        // Add new
        if (tomatoCounter is null)
        {
            var client = await context.Players
                .Where(x => x.Identity == identity)
                .Select(x => new {x.Id})
                .FirstOrDefaultAsync();
            if (client is null) return;

            var tomatoContext = new EFTomatoCounter
            {
                Tomatoes = count,
                PlayerId = client.Id,
            };

            context.TomatoCounters.Add(tomatoContext);
            await context.SaveChangesAsync();
            return;
        }

        // Update existing
        tomatoCounter.Tomatoes = count;
        await context.SaveChangesAsync();
    }
}
