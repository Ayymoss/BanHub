using BanHub.WebCore.Client.Services.RestEase;
using BanHub.WebCore.Shared.Models.Shared;
using Microsoft.AspNetCore.Components;

namespace BanHub.WebCore.Client.Pages;

partial class Index
{
    [Inject] protected StatisticService StatisticService { get; set; }

    private Statistic _statistic = new();

    protected override async Task OnInitializedAsync()
    {
        _statistic = await StatisticService.GetStatisticAsync();
        await base.OnInitializedAsync();
    }
}
