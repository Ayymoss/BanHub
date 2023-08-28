using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Chat;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileChatTable
{
    [Parameter] public required Player Player { get; set; }

    [Inject] protected PlayerProfileService PlayerProfileService { get; set; }
    [Inject] protected ChatService ChatService { get; set; }

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
            Sorts = args.Sorts,
            SearchString = _searchString,
            Top = args.Top ?? 10,
            Skip = args.Skip ?? 0,
            Identity = Player.Identity
        };

        var context = await PlayerProfileService.GetProfileChatPaginationAsync(paginationQuery);
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

        var chatContext = await ChatService.GetChatContextAsync(new GetMessageContextCommand
        {
            Submitted = message.Submitted,
            ServerId = message.ServerId
        });

        _chatContext[message.Submitted] = chatContext;
    }
}
