using BanHub.WebCore.Client.Services.RestEase;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Models.IndexView;
using BanHub.WebCore.Shared.Models.Shared;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Pages;

partial class Index
{
    [Inject] protected StatisticService StatisticService { get; set; }
    [Inject] protected PenaltyService PenaltyService { get; set; }

    private RadzenDataGrid<Penalty> _dataGrid;
    private IEnumerable<Penalty> _indexTable;
    private bool _isLoading = true;
    private int _count;

    private Statistic _statistic = new();

    protected override async Task OnInitializedAsync()
    {
        _statistic = await StatisticService.GetStatisticAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadData(LoadDataArgs args)
    {
        _indexTable = await PenaltyService.GetLatestBansAsync();
        _count = _indexTable.Count();
        _isLoading = false;
    }
}
