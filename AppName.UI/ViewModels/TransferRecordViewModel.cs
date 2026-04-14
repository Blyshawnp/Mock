using AppName.Core.Models.Enums;
using AppName.Core.Models.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppName.UI.ViewModels;

public partial class TransferRecordViewModel : ViewModelBase
{
    private readonly Action<TransferRecordViewModel> _onChanged;
    private bool _isInitializing;

    public TransferRecordViewModel(TransferRecord model, Action<TransferRecordViewModel> onChanged)
    {
        _onChanged = onChanged;
        _isInitializing = true;

        AttemptNumber = model.AttemptNumber;
        Outcome = model.Outcome;
        Reason = model.Reason ?? string.Empty;
        Notes = model.Notes ?? string.Empty;
        FollowUpRequired = model.FollowUpRequired;
        FollowUpDate = model.FollowUpDate?.LocalDateTime;

        _isInitializing = false;
    }

    public int AttemptNumber { get; }

    [ObservableProperty]
    private TransferOutcome outcome;

    [ObservableProperty]
    private string reason = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private bool followUpRequired;

    [ObservableProperty]
    private DateTime? followUpDate;

    partial void OnOutcomeChanged(TransferOutcome value) => NotifyChanged();
    partial void OnReasonChanged(string value) => NotifyChanged();
    partial void OnNotesChanged(string value) => NotifyChanged();
    partial void OnFollowUpRequiredChanged(bool value) => NotifyChanged();
    partial void OnFollowUpDateChanged(DateTime? value) => NotifyChanged();

    public TransferRecord ToModel()
    {
        return new TransferRecord
        {
            AttemptNumber = AttemptNumber,
            Outcome = Outcome,
            Reason = Reason,
            Notes = Notes,
            FollowUpRequired = FollowUpRequired,
            FollowUpDate = FollowUpDate.HasValue
    ? new DateTimeOffset(FollowUpDate.Value)
    : null
        };
    }

    private void NotifyChanged()
    {
        if (_isInitializing)
        {
            return;
        }

        _onChanged.Invoke(this);
    }
}