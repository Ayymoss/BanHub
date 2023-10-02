using BanHub.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components;

partial class EnumMudChip
{
    [Parameter] public required Enum Penalty { get; set; }

    private readonly Dictionary<Enum, BadgeStyle> _enumDetails = new()
    {
        {PenaltyType.Report, BadgeStyle.Light},
        {PenaltyType.Warning, BadgeStyle.Light},
        {PenaltyType.Mute, BadgeStyle.Light},
        {PenaltyType.Kick, BadgeStyle.Light},
        {PenaltyType.Unban, BadgeStyle.Info},
        {PenaltyType.TempBan, BadgeStyle.Warning},
        {PenaltyType.Ban, BadgeStyle.Danger},
        {PenaltyStatus.Informational, BadgeStyle.Info},
        {PenaltyStatus.Active, BadgeStyle.Success},
        {PenaltyStatus.Revoked, BadgeStyle.Info},
        {PenaltyStatus.Expired, BadgeStyle.Info},
        {PenaltyScope.Local, BadgeStyle.Info},
        {PenaltyScope.Global, BadgeStyle.Danger},
    };
}
