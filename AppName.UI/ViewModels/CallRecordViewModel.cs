using AppName.Core.Models.Enums;
using AppName.Core.Models.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppName.UI.ViewModels;

public partial class CallRecordViewModel : ViewModelBase
{
    private const string OtherValue = "Other";
    private readonly Action<CallRecordViewModel> _onChanged;
    private bool _isInitialized;

    public CallRecordViewModel(CallRecord model, Action<CallRecordViewModel> onChanged)
    {
        _onChanged = onChanged;

        CallNumber = model.CallNumber;
        DonorScenario = model.DonorScenario;
        Outcome = model.Outcome;
        SelectedCoachingReason = model.CoachingSelections?.FirstOrDefault() ?? string.Empty;
        SelectedFailReason = model.FailSelections?.FirstOrDefault() ?? string.Empty;
        OtherCoachingText = model.OtherCoachingText ?? string.Empty;
        OtherFailText = model.OtherFailText ?? string.Empty;
        Notes = model.Notes ?? string.Empty;
        IsVisible = model.IsVisible;

        _isInitialized = true;
    }

    public int CallNumber { get; }

    [ObservableProperty]
    private DonorScenario donorScenario;

    [ObservableProperty]
    private CallOutcome outcome;

    [ObservableProperty]
    private string selectedCoachingReason = string.Empty;

    [ObservableProperty]
    private string selectedFailReason = string.Empty;

    [ObservableProperty]
    private string otherCoachingText = string.Empty;

    [ObservableProperty]
    private string otherFailText = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private bool isVisible = true;

    public bool ShowOtherCoachingText =>
        SelectedCoachingReason.Equals(OtherValue, StringComparison.OrdinalIgnoreCase);

    public bool ShowOtherFailText =>
        SelectedFailReason.Equals(OtherValue, StringComparison.OrdinalIgnoreCase);

    partial void OnDonorScenarioChanged(DonorScenario value) => NotifyChanged();

    partial void OnOutcomeChanged(CallOutcome value) => NotifyChanged();

    partial void OnSelectedCoachingReasonChanged(string value)
    {
        OnPropertyChanged(nameof(ShowOtherCoachingText));
        NotifyChanged();
    }

    partial void OnSelectedFailReasonChanged(string value)
    {
        OnPropertyChanged(nameof(ShowOtherFailText));
        NotifyChanged();
    }

    partial void OnOtherCoachingTextChanged(string value) => NotifyChanged();

    partial void OnOtherFailTextChanged(string value) => NotifyChanged();

    partial void OnNotesChanged(string value) => NotifyChanged();

    public CallRecord ToModel()
    {
        var coaching = string.IsNullOrWhiteSpace(SelectedCoachingReason)
            ? []
            : [SelectedCoachingReason];

        var failReasons = string.IsNullOrWhiteSpace(SelectedFailReason)
            ? []
            : [SelectedFailReason];

        return new CallRecord
        {
            CallNumber = CallNumber,
            Outcome = Outcome,
            DonorScenario = DonorScenario,
            CoachingSelections = coaching,
            FailSelections = failReasons,
            OtherCoachingText = ShowOtherCoachingText ? OtherCoachingText : null,
            OtherFailText = ShowOtherFailText ? OtherFailText : null,
            Notes = Notes,
            IsVisible = IsVisible,
            IsComplete = Outcome != CallOutcome.NotScored
        };
    }

    private void NotifyChanged()
    {
        if (!_isInitialized)
        {
            return;
        }

        _onChanged.Invoke(this);
    }
}
