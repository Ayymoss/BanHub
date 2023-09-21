using BanHub.WebCore.Client.Services.RestEase;
using BanHub.WebCore.Shared.Models.Shared;
using Microsoft.AspNetCore.Components;

namespace BanHub.WebCore.Client.Pages;

partial class Index(StatisticService statisticService)
{
    private Statistic _statistic = new();

    protected override async Task OnInitializedAsync()
    {
        _statistic = await statisticService.GetStatisticAsync();
        await base.OnInitializedAsync();
    }
}
