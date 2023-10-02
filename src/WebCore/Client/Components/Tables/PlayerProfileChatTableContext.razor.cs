using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;

namespace BanHub.WebCore.Client.Components.Tables;

partial class PlayerProfileChatTableContext
{
    [Parameter] public required Chat Chat { get; set; }
    [Parameter] public required ChatContextRoot Context { get; set; }
}
