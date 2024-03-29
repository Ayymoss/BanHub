﻿using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SortDescriptor = BanHub.Domain.ValueObjects.Services.SortDescriptor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileNoteTable(PlayerProfileService playerProfileService, DialogService dialogService)
{
    [Parameter] public required Player Player { get; set; }
    [Parameter] public required bool Privileged { get; set; }

    private RadzenDataGrid<Note> _dataGrid;
    private IEnumerable<Note> _data;
    private bool _loading = true;
    private int _totalCount;
    private string? _searchString;

    private async Task LoadData(LoadDataArgs args)
    {
        _loading = true;
        var paginationQuery = new GetProfileNotesPaginationCommand
        {
            Sorts = args.Sorts.Select(x => new SortDescriptor
            {
                Property = x.Property,
                SortOrder = x.SortOrder == SortOrder.Ascending
                    ? SortDirection.Ascending
                    : SortDirection.Descending
            }),
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Player.Identity
        };

        var context = await playerProfileService.GetProfileNotesPaginationAsync(paginationQuery);
        _data = context.Data;
        _totalCount = context.Count;
        _loading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }

    private async Task RowClickEvent(DataGridRowMouseEventArgs<Note> arg)
    {
        if (!Privileged) return;

        var parameters = new Dictionary<string, object>
        {
            {"Note", arg.Data}
        };

        var options = new DialogOptions
        {
            Style = "min-height:auto;min-width:auto;width:auto;max-width:75%;max-height:97%",
            CloseDialogOnOverlayClick = true
        };

        var dialog = await dialogService.OpenAsync<ProfileDeleteNoteDialog>("Delete Note?", parameters, options);

        if (dialog is Note note) await _dataGrid.Reload();
    }
}
