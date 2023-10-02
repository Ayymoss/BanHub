using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Chat.Commands;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Enums;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using SortDescriptor = BanHub.Domain.ValueObjects.Services.SortDescriptor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileChatTable(PlayerProfileService playerProfileService, ChatService chatService)
{
    [Parameter] public required Player Player { get; set; }

    private RadzenDataGrid<Chat> _dataGrid;
    private IEnumerable<Chat> _data;
    private bool _loading = true;
    private int _totalCount;
    private string? _searchString;

    private readonly Dictionary<DateTimeOffset, ChatContextRoot> _chatContext = new();
    private readonly Dictionary<DateTimeOffset, bool> _chatClickStates = new();

    private async Task LoadData(LoadDataArgs args)
    {
        _loading = true;
        var paginationQuery = new GetProfileChatPaginationCommand
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

        var context = await playerProfileService.GetProfileChatPaginationAsync(paginationQuery);
        _data = context.Data;
        _totalCount = context.Count;
        _loading = false;
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _dataGrid.Reload();
    }

    private string GetChatIconState(DateTimeOffset submitted)
    {
        if (_chatClickStates.TryGetValue(submitted, out bool state))
        {
            return state ? "expand_more" : "chevron_right";
        }

        return "chevron_right";
    }

    private async Task OnChatClick(MouseEventArgs arg, Chat message)
    {
        if (_chatClickStates.ContainsKey(message.Submitted))
        {
            _chatClickStates[message.Submitted] = !_chatClickStates[message.Submitted];
        }

        foreach (var state in _chatClickStates.Where(x => x.Value))
        {
            if (state.Key == message.Submitted) continue;
            _chatClickStates[state.Key] = false;
        }

        if (_chatContext.TryGetValue(message.Submitted, out var value) && value.Loaded) return;

        var chatContext = await chatService.GetChatContextAsync(new GetMessageContextCommand
        {
            Submitted = message.Submitted,
            ServerId = message.ServerId
        });

        _chatContext[message.Submitted] = chatContext;
    }
}
