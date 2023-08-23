using BanHub.WebCore.Client.SignalR;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Shared;

partial class MainLayout
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
        StateHasChanged();
    }

    private void UpdateOnlineCount(int count)
    {
        _online = count;
        StateHasChanged();
    }

    private void UpdateBansCount(int count)
    {
        _bans = count;
        StateHasChanged();
    }

    private async Task ToggleTheme()
    {
        await JsRuntime.InvokeVoidAsync("toggleTheme");
        _isDarkTheme = !_isDarkTheme;
    }
}
