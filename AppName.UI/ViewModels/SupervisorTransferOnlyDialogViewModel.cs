using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SupervisorTransferOnlyDialogViewModel : ViewModelBase
{
    public SupervisorTransferOnlyDialogViewModel()
    {
        Candidates = [];

        FilteredCandidates = [];
        RefreshFiltered();
    }

    public ObservableCollection<CandidateRow> Candidates { get; }

    public ObservableCollection<CandidateRow> FilteredCandidates { get; }

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private CandidateRow? selectedCandidate;

    [ObservableProperty]
    private bool? conductedMockCallSession;

    [ObservableProperty]
    private bool showInitialQuestionDialog;

    [ObservableProperty]
    private bool showCandidatePickerDialog;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [RelayCommand]
    private void OpenInitialQuestion()
    {
        ShowInitialQuestionDialog = true;
    }

    [RelayCommand]
    private void ConfirmInitialQuestion(string answer)
    {
        ShowInitialQuestionDialog = false;
        ConductedMockCallSession = string.Equals(answer, "Yes", StringComparison.OrdinalIgnoreCase);

        if (ConductedMockCallSession == true)
        {
            ShowCandidatePickerDialog = true;
            StatusMessage = "Select candidate from the last 10 sessions.";
        }
        else
        {
            StatusMessage = "Proceeding without history load.";
        }
    }

    [RelayCommand]
    private void CancelCandidatePicker()
    {
        ShowCandidatePickerDialog = false;
        SelectedCandidate = null;
        SearchText = string.Empty;
        RefreshFiltered();
    }

    [RelayCommand(CanExecute = nameof(CanConfirmCandidatePicker))]
    private void ConfirmCandidatePicker()
    {
        if (SelectedCandidate is null)
        {
            return;
        }

        ShowCandidatePickerDialog = false;
        StatusMessage = $"Loaded {SelectedCandidate.CandidateName} ({SelectedCandidate.TesterName}).";
    }

    partial void OnSearchTextChanged(string value)
    {
        RefreshFiltered();
    }

    partial void OnSelectedCandidateChanged(CandidateRow? value)
    {
        ConfirmCandidatePickerCommand.NotifyCanExecuteChanged();
    }

    private bool CanConfirmCandidatePicker() => SelectedCandidate is not null;

    private void RefreshFiltered()
    {
        FilteredCandidates.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Candidates
            : Candidates.Where(x =>
                x.CandidateName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                x.TesterName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var row in filtered.Take(10))
        {
            FilteredCandidates.Add(row);
        }
    }

    public sealed record CandidateRow(string CandidateName, string TesterName);
}
