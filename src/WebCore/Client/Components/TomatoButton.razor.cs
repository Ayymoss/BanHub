using BanHub.WebCore.Client.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BanHub.WebCore.Client.Components;

partial class TomatoButton(TomatoCounterHub tomatoCounterHub) : IAsyncDisposable
{
    [Parameter] public required string Identity { get; set; }

    private int _tomatoCount;
    private readonly List<Tomato> _tomatoes = new();

    protected override async Task OnInitializedAsync()
    {
        await InitializeSignalRHubs();
        await base.OnInitializedAsync();
    }

    private void DropTomatoes()
    {
        for (var i = 0; i < 5; i++)
        {
            var size = Random.Shared.Next(20, 50);
            var isRotten = Random.Shared.NextDouble() < 0.2;
            var rotationDirection = Random.Shared.Next(0, 2) == 0 ? "clockwise" : "counter-clockwise";
            var rotationSpeed = 0.4 + 1.6 * Random.Shared.NextDouble();

            _tomatoes.Add(new Tomato
            {
                Id = Guid.NewGuid().ToString(),
                Size = size,
                IsRotten = isRotten,
                RotationDirection = rotationDirection,
                RotationSpeed = rotationSpeed,
                FallSpeed = (int)(10.0 - 5.0 * size / 50.0),
                LeftPosition = Random.Shared.Next(1, 80),
                TopPosition = -Random.Shared.Next(10, 20),
                CreatedOn = DateTimeOffset.UtcNow
            });
        }
    }

    private void CleanupOldTomatoes()
    {
        var now = DateTimeOffset.UtcNow;
        var oldTomatoes = _tomatoes.Where(t => (now - t.CreatedOn).TotalSeconds > 10).ToList();
        foreach (var oldTomato in oldTomatoes)
        {
            oldTomato.LeftPosition = 1;
            // I don't know how to remove the objects without the animation rendering again for all objects. :(
            // This is my solution to move them to the far left so any issues with horizontal scrolling are avoided.
        }
    }

    private async Task InitializeSignalRHubs()
    {
        SubscribeToHubEvents();
        await tomatoCounterHub.InitializeAsync(Identity);
    }

    private void SubscribeToHubEvents()
    {
        tomatoCounterHub.TomatoCountChanged += TomatoCountChanged;
    }

    private void TomatoCountChanged(int count)
    {
        _tomatoCount = count;
        DropTomatoes();
        CleanupOldTomatoes();
        StateHasChanged();
    }

    private async Task IncrementCount()
    {
        _tomatoCount++;
        await tomatoCounterHub.IncrementCount(Identity);
        StateHasChanged();
    }

    private class Tomato
    {
        public string Id { get; set; }
        public int Size { get; set; }
        public bool IsRotten { get; set; }
        public string RotationDirection { get; set; }
        public double RotationSpeed { get; set; }
        public int FallSpeed { get; set; }
        public int LeftPosition { get; set; }
        public int TopPosition { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }

    public async ValueTask DisposeAsync()
    {
        await tomatoCounterHub.DisposeAsync();
    }
}
