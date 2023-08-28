using BanHub.WebCore.Client.Components.Dialogs;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileNoteTable
{
    [Parameter] public required Player Player { get; set; }
    [Parameter] public required bool Privileged { get; set; }

    [Inject] protected PlayerProfileService PlayerProfileService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }

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
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Player.Identity
        };

        var context = await PlayerProfileService.GetProfileNotesPaginationAsync(paginationQuery);
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

        var dialog = await DialogService.OpenAsync<ProfileDeleteNoteDialog>("Delete Note?", parameters, options);

        if (dialog is Note note) await _dataGrid.Reload();
    }
}
