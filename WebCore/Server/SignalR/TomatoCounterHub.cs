using System.Collections.Concurrent;
using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.SignalR;

public class TomatoCounterHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> TomatoCounters = new();

    private readonly DataContext _context;
    private readonly SemaphoreSlim _tomatoLock = new(1, 1);

    public TomatoCounterHub(DataContext context)
    {
        _context = context;
    }

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

            await Clients.Group(identity).SendAsync(HubMethods.OnTomatoCountUpdate, count);
            if (count % 123 is 0) await StoreTomatoes(identity);
        }
        finally
        {
            if (_tomatoLock.CurrentCount is 0) _tomatoLock.Release();
        }
    }

    private async Task LoadTomatoes(string identity)
    {
        var tomatoCounter = await _context.TomatoCounters
            .AsNoTracking()
            .Where(x => x.Player.Identity == identity)
            .FirstOrDefaultAsync();

        if (tomatoCounter is not null) TomatoCounters[identity] = tomatoCounter.Tomatoes;
    }

    private async Task StoreTomatoes(string identity)
    {
        if (!TomatoCounters.TryGetValue(identity, out var count)) return;

        var tomatoCounter = await _context.TomatoCounters
            .Where(x => x.Player.Identity == identity)
            .FirstOrDefaultAsync();

        // Add new
        if (tomatoCounter is null)
        {
            var client = await _context.Players
                .Where(x => x.Identity == identity)
                .Select(x => new {x.Id})
                .FirstOrDefaultAsync();
            if (client is null) return;

            var tomatoContext = new EFTomatoCounter
            {
                Tomatoes = count,
                PlayerId = client.Id,
            };

            _context.TomatoCounters.Add(tomatoContext);
            await _context.SaveChangesAsync();
            return;
        }

        // Update existing
        tomatoCounter.Tomatoes = count;
        await _context.SaveChangesAsync();
    }
}
