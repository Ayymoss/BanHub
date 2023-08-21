using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using Microsoft.AspNetCore.Components;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileChatTableContext
{
    [Parameter] public required Chat Chat { get; set; }
    [Parameter] public required ChatContextRoot Context { get; set; }

    [Inject] protected ChatService ChatService { get; set; }
}
