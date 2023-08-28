using BanHub.WebCore.Client.SignalR;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Shared;

partial class MainLayout : IDisposable
{
    [Inject] protected ActiveUserHub ActiveUserHub { get; set; }
    [Inject] protected StatisticsHub StatisticsHub { get; set; }
    [Inject] protected ILocalStorageService LocalStorage { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }

    private int _online;
    private int _bans;
    private bool _drawerOpen = true;
    private int _activeUserCount;
    private RadzenBody _body;
    private bool _isDarkTheme;

    // Animation
    private int _startOnline;
    private int _targetOnline;
    private Timer? _updateTimer;
    private const int UpdateInterval = 100;
    private const double Step = 0.02;
    private double _progress;

    private string ThemeButtonClass => $"{(_isDarkTheme ? "rz-info" : "rz-warning")} rz-mr-4";

    protected override async Task OnInitializedAsync()
    {
        var currentTheme = await LocalStorage.GetItemAsync<string>("theme");
        _isDarkTheme = currentTheme?.Contains("dark") ?? true;
        await InitializeSignalRHubs();
        await base.OnInitializedAsync();
    }

    private async Task InitializeSignalRHubs()
    {
        SubscribeToHubEvents();
        await ActiveUserHub.InitializeAsync();
        await StatisticsHub.InitializeAsync();
    }

    private void SubscribeToHubEvents()
    {
        ActiveUserHub.ActiveUserCountChanged += UpdatePageViewersCount;
        StatisticsHub.OnlineCountChanged += UpdateOnlineCount;
        StatisticsHub.RecentBansCountChanged += UpdateBansCount;
    }

    private void UpdatePageViewersCount(int count)
    {
        _activeUserCount = count;
        InvokeAsync(StateHasChanged);
    }

    private void UpdateOnlineCount(int count)
    {
        _startOnline = _online;
        _targetOnline = count;
        _progress = 0;
        _updateTimer ??= new Timer(AnimateCount, null, 0, UpdateInterval);
    }

    private void UpdateBansCount(int count)
    {
        _bans = count;
        InvokeAsync(StateHasChanged);
    }

    private async Task ToggleTheme()
    {
        await JsRuntime.InvokeVoidAsync("toggleTheme");
        _isDarkTheme = !_isDarkTheme;
    }

    private void AnimateCount(object? state)
    {
        _progress += Step;

        if (_progress >= 1)
        {
            _online = _targetOnline;
            _updateTimer?.Dispose();
            _updateTimer = null;
            InvokeAsync(StateHasChanged);
            return;
        }

        var easedProgress = BezierEase(_progress);
        _online = _startOnline + (int)Math.Round((_targetOnline - _startOnline) * easedProgress);
        InvokeAsync(StateHasChanged);
    }

    private static double BezierEase(double t)
    {
        var u = 1 - t;
        var tt = t * t;
        var uu = u * u;
        var uuu = uu * u;
        var ttt = tt * t;

        var y = uuu * 0 // P0 is always (0,0) so it's omitted here
                + 3 * uu * t * 0.1 // P1 is (0.25, 0.1) - we don't use the x value
                + 3 * u * tt * 1 // P2 is (0.25, 1) - we don't use the x value
                + ttt; // P3 is always (1,1) so it's just t^3

        return y;
    }

    public void Dispose()
    {
        _body.Dispose();
        _updateTimer?.Dispose();
        ActiveUserHub.ActiveUserCountChanged -= UpdatePageViewersCount;
        StatisticsHub.OnlineCountChanged -= UpdateOnlineCount;
        StatisticsHub.RecentBansCountChanged -= UpdateBansCount;
    }
}
