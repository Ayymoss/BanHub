using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Models.IndexView;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class IndexPenaltyPreviewTable
{
    [Inject] protected PenaltyService PenaltyService { get; set; }

    private RadzenDataGrid<Penalty> _dataGrid;
    private IEnumerable<Penalty> _indexTable;
    private bool _isLoading = true;
    private int _count;
    
    private async Task LoadData(LoadDataArgs args)
    {
        _indexTable = await PenaltyService.GetLatestBansAsync();
        _count = _indexTable.Count();
        _isLoading = false;
    }
}
