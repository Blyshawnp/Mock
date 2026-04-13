using AppName.Core.Models.Enums;
using AppName.Core.Models.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppName.UI.ViewModels;

public partial class TransferRecordViewModel : ViewModelBase
{
    private readonly Action<TransferRecordViewModel> _onChanged;

    public TransferRecordViewModel(TransferRecord model, Action<TransferRecordViewModel> onChanged)
    {
        _onChanged = onChanged;

        AttemptNumber = model.AttemptNumber;
        Outcome = model.Outcome;
        Reason = model.Reason ?? string.Empty;
        Notes = model.Notes ?? string.Empty;
        FollowUpRequired = model.FollowUpRequired;
        FollowUpDate = model.FollowUpDate;
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
    private DateTimeOffset? followUpDate;

    public DateTime? FollowUpDateForPicker
    {
        get => FollowUpDate?.DateTime;
        set
        {
            FollowUpDate = value is null ? null : new DateTimeOffset(value.Value);
            OnPropertyChanged();
            NotifyChanged();
        }
    }

    public bool ShowFollowUpFields => Outcome == TransferOutcome.Fail;

    partial void OnOutcomeChanged(TransferOutcome value)
    {
        OnPropertyChanged(nameof(ShowFollowUpFields));

        if (value != TransferOutcome.Fail)
        {
            FollowUpRequired = false;
            FollowUpDate = null;
        }

        OnPropertyChanged(nameof(FollowUpDateForPicker));
        NotifyChanged();
    }

    partial void OnReasonChanged(string value) => NotifyChanged();

    partial void OnNotesChanged(string value) => NotifyChanged();

    partial void OnFollowUpRequiredChanged(bool value) => NotifyChanged();

    partial void OnFollowUpDateChanged(DateTimeOffset? value)
    {
        OnPropertyChanged(nameof(FollowUpDateForPicker));
        NotifyChanged();
    }

    public TransferRecord ToModel()
    {
        return new TransferRecord
        {
            AttemptNumber = AttemptNumber,
            Outcome = Outcome,
            Reason = Reason,
            Notes = Notes,
            FollowUpRequired = ShowFollowUpFields && FollowUpRequired,
            FollowUpDate = ShowFollowUpFields && FollowUpRequired ? FollowUpDate : null
        };
    }

    private void NotifyChanged()
    {
        _onChanged.Invoke(this);
    }
}
