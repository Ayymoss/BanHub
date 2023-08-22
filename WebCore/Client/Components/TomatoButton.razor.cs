using BanHub.WebCore.Client.SignalR;
using Microsoft.AspNetCore.Components;

namespace BanHub.WebCore.Client.Components;

partial class TomatoButton
{
    [Parameter] public required string Identity { get; set; }

    [Inject] protected TomatoCounterHub TomatoCounterHub { get; set; }

    private int _tomatoCount;
    private readonly Queue<Tomato> _tomatoes = new();

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

            _tomatoes.Enqueue(new Tomato
            {
                Id = Guid.NewGuid().ToString(),
                Size = size,
                IsRotten = isRotten,
                RotationDirection = rotationDirection,
                RotationSpeed = rotationSpeed,
                FallSpeed = (int)(10.0 - 5.0 * size / 50.0),
                LeftPosition = Random.Shared.Next(1, 80),
                TopPosition = -Random.Shared.Next(10, 20)
            });
        }
    }

    private async Task InitializeSignalRHubs()
    {
        SubscribeToHubEvents();
        await TomatoCounterHub.InitializeAsync(Identity);
    }

    private void SubscribeToHubEvents()
    {
        TomatoCounterHub.TomatoCountChanged += TomatoCountChanged;
    }

    private void TomatoCountChanged(int count)
    {
        _tomatoCount = count;
        DropTomatoes();
        StateHasChanged();
    }

    private async Task IncrementCount()
    {
        _tomatoCount++;
        await TomatoCounterHub.IncrementCount(Identity);
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
    }
}
