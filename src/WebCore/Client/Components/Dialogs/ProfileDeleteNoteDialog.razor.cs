using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Player.Commands;
using BanHub.WebCore.Client.Services.RestEase.Pages;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace BanHub.WebCore.Client.Components.Dialogs;

partial class ProfileDeleteNoteDialog(NotificationService notificationService, DialogService dialogService, NoteService noteService)
{
    [Parameter] public required Note Note { get; set; }

    private string _deletionReason = string.Empty;
    private bool _processing;

    private async Task DeletePenalty()
    {
        _processing = true;

        if (_deletionReason.Length < 3)
        {
            notificationService.Notify(NotificationSeverity.Warning, "A reason longer than 3 chars required!");
            _processing = false;
            return;
        }

        var request = await noteService.DeleteNoteAsync(new DeleteNoteCommand
        {
            DeletionReason = _deletionReason,
            NoteGuid = Note.NoteGuid
        });
        if (!request)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed to delete note!");
            _processing = false;
            dialogService.Close();
            return;
        }

        notificationService.Notify(NotificationSeverity.Success, "Note deleted!");
        _processing = false;
        dialogService.Close(Note);
    }
}
