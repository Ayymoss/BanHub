﻿using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;
using BanHub.WebCore.Shared.Models.ServersView;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Pages;

partial class Servers
{
    [Inject] protected ServerService ServerService { get; set; }

    private RadzenDataGrid<Server> _dataGrid;
    private IEnumerable<Server> _playerTable;
    private bool _isLoading = true;
    private int _count;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _isLoading = true;
        var paginationQuery = new GetServersPaginationCommand
        {
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 20,
            Skip = args.Skip ?? 0
        };

        var context = await ServerService.GetServersPaginationAsync(paginationQuery);
        _playerTable = context.Data;
        _count = context.Count;
        _isLoading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }
}
