using BanHub.WebCore.Client.Services.RestEase.Pages;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfileDeleteNoteDialog
{
    [Parameter] public required Note Note { get; set; }
    [Inject] protected NotificationService NotificationService { get; set; }
    [Inject] protected DialogService DialogService { get; set; }
    [Inject] protected NoteService NoteService { get; set; }

    private string _deletionReason = string.Empty;
    private bool _processing;

    private async Task DeletePenalty()
    {
        _processing = true;

        if (_deletionReason.Length < 3)
        {
            NotificationService.Notify(NotificationSeverity.Warning, "A reason longer than 3 chars required!");
            _processing = false;
            return;
        }

        var request = await NoteService.DeleteNoteAsync(new DeleteNoteCommand
        {
            DeletionReason = _deletionReason,
            NoteGuid = Note.NoteGuid
        });
        if (!request)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Failed to delete note!");
            _processing = false;
            DialogService.Close();
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "Note deleted!");
        _processing = false;
        DialogService.Close(Note);
    }
}
