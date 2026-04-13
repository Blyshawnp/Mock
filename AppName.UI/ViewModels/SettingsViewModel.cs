using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Lookup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ILookupTableService _lookupTableService;

    public SettingsViewModel(ILookupTableService lookupTableService)
    {
        _lookupTableService = lookupTableService;

        CallTypes = [];
        SupervisorReasons = [];
        FailReasons = [];
        CoachingCategories = [];
        ShowOffers = [];
        DonorProfiles = [];
        ValidationErrors = [];
    }

    public ObservableCollection<LookupItem> CallTypes { get; }

    public ObservableCollection<LookupItem> SupervisorReasons { get; }

    public ObservableCollection<LookupItem> FailReasons { get; }

    public ObservableCollection<LookupItem> CoachingCategories { get; }

    public ObservableCollection<ShowOfferSeed> ShowOffers { get; }

    public ObservableCollection<DonorProfileSeed> DonorProfiles { get; }

    public ObservableCollection<string> ValidationErrors { get; }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isInitialized;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private ShowOfferSeed? selectedShowOffer;

    [ObservableProperty]
    private DonorProfileSeed? selectedDonorProfile;

    [ObservableProperty]
    private bool hasValidationErrors;

    [RelayCommand(CanExecute = nameof(CanInitialize))]
    private async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        await LoadAsync();
        IsInitialized = true;
    }

    [RelayCommand(CanExecute = nameof(CanLoadOrSave))]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var tables = await _lookupTableService.GetLookupTablesAsync();

            Reload(CallTypes, tables.CallTypes);
            Reload(SupervisorReasons, tables.SupervisorReasons);
            Reload(FailReasons, tables.FailReasons);
            Reload(CoachingCategories, tables.CoachingCategories);
            Reload(ShowOffers, tables.ShowOffers);
            Reload(DonorProfiles, tables.DonorProfiles);

            StatusMessage = "Lookup tables loaded.";
            ClearValidation();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadOrSave))]
    private async Task SaveAsync()
    {
        IsBusy = true;
        try
        {
            if (!ValidateAll())
            {
                StatusMessage = "Cannot save. Fix validation errors.";
                return;
            }

            NormalizeSort(CallTypes);
            NormalizeSort(SupervisorReasons);
            NormalizeSort(FailReasons);
            NormalizeSort(CoachingCategories);
            NormalizeSort(ShowOffers);
            NormalizeSort(DonorProfiles);

            var lookup = new LookupTableSet
            {
                CallTypes = [.. CallTypes],
                SupervisorReasons = [.. SupervisorReasons],
                FailReasons = [.. FailReasons],
                CoachingCategories = [.. CoachingCategories],
                ShowOffers = [.. ShowOffers],
                DonorProfiles = [.. DonorProfiles]
            };

            await _lookupTableService.SaveLookupTablesAsync(lookup);
            StatusMessage = "Lookup settings saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanLoadOrSave))]
    private void AddShowOffer()
    {
        var item = new ShowOfferSeed
        {
            ShowName = "New Show",
            OneTimeAmount = "$0",
            MonthlyAmount = "$0",
            GiftDescription = string.Empty,
            IsEnabled = true,
            SortOrder = ShowOffers.Count + 1
        };

        ShowOffers.Add(item);
        SelectedShowOffer = item;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveShowOffer))]
    private void RemoveShowOffer()
    {
        if (SelectedShowOffer is null)
        {
            return;
        }

        ShowOffers.Remove(SelectedShowOffer);
        NormalizeSort(ShowOffers);
        SelectedShowOffer = null;
    }

    [RelayCommand(CanExecute = nameof(CanLoadOrSave))]
    private void AddDonorProfile()
    {
        var item = new DonorProfileSeed
        {
            DonorScenario = DonorScenario.NewDonor,
            FirstName = "First",
            LastName = "Last",
            IsEnabled = true,
            SortOrder = DonorProfiles.Count + 1
        };

        DonorProfiles.Add(item);
        SelectedDonorProfile = item;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveDonorProfile))]
    private void RemoveDonorProfile()
    {
        if (SelectedDonorProfile is null)
        {
            return;
        }

        DonorProfiles.Remove(SelectedDonorProfile);
        NormalizeSort(DonorProfiles);
        SelectedDonorProfile = null;
    }

    partial void OnIsBusyChanged(bool value)
    {
        InitializeCommand.NotifyCanExecuteChanged();
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        AddShowOfferCommand.NotifyCanExecuteChanged();
        RemoveShowOfferCommand.NotifyCanExecuteChanged();
        AddDonorProfileCommand.NotifyCanExecuteChanged();
        RemoveDonorProfileCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedShowOfferChanged(ShowOfferSeed? value)
    {
        RemoveShowOfferCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedDonorProfileChanged(DonorProfileSeed? value)
    {
        RemoveDonorProfileCommand.NotifyCanExecuteChanged();
    }

    private bool CanInitialize() => !IsBusy && !IsInitialized;

    private bool CanLoadOrSave() => !IsBusy;

    private bool CanRemoveShowOffer() => !IsBusy && SelectedShowOffer is not null;

    private bool CanRemoveDonorProfile() => !IsBusy && SelectedDonorProfile is not null;

    private bool ValidateAll()
    {
        ValidationErrors.Clear();

        ValidateLookupCollection(CallTypes, "Call Types");
        ValidateLookupCollection(SupervisorReasons, "Supervisor Reasons");
        ValidateLookupCollection(FailReasons, "Fail Reasons");
        ValidateLookupCollection(CoachingCategories, "Coaching Categories");
        ValidateShowOffers();
        ValidateDonorProfiles();

        HasValidationErrors = ValidationErrors.Count > 0;
        return !HasValidationErrors;
    }

    private void ValidateLookupCollection(IEnumerable<LookupItem> items, string collectionName)
    {
        var index = 1;
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                ValidationErrors.Add($"{collectionName} row {index}: Name is required.");
            }

            index++;
        }
    }

    private void ValidateShowOffers()
    {
        var index = 1;
        foreach (var show in ShowOffers)
        {
            if (string.IsNullOrWhiteSpace(show.ShowName))
            {
                ValidationErrors.Add($"Show Offers row {index}: Show Name is required.");
            }

            if (string.IsNullOrWhiteSpace(show.OneTimeAmount))
            {
                ValidationErrors.Add($"Show Offers row {index}: One-Time amount is required.");
            }

            if (string.IsNullOrWhiteSpace(show.MonthlyAmount))
            {
                ValidationErrors.Add($"Show Offers row {index}: Monthly amount is required.");
            }

            index++;
        }
    }

    private void ValidateDonorProfiles()
    {
        var index = 1;
        foreach (var donor in DonorProfiles)
        {
            if (string.IsNullOrWhiteSpace(donor.FirstName))
            {
                ValidationErrors.Add($"Donor Profiles row {index}: First name is required.");
            }

            if (string.IsNullOrWhiteSpace(donor.LastName))
            {
                ValidationErrors.Add($"Donor Profiles row {index}: Last name is required.");
            }

            if (string.IsNullOrWhiteSpace(donor.Email))
            {
                ValidationErrors.Add($"Donor Profiles row {index}: Email is required.");
            }

            index++;
        }
    }

    private void ClearValidation()
    {
        ValidationErrors.Clear();
        HasValidationErrors = false;
    }

    private static void Reload<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private static void NormalizeSort(IEnumerable<LookupItem> list)
    {
        var sorted = list.OrderBy(x => x.SortOrder).ToList();
        for (var index = 0; index < sorted.Count; index++)
        {
            sorted[index].SortOrder = index + 1;
        }
    }

    private static void NormalizeSort(IEnumerable<ShowOfferSeed> list)
    {
        var sorted = list.OrderBy(x => x.SortOrder).ToList();
        for (var index = 0; index < sorted.Count; index++)
        {
            sorted[index].SortOrder = index + 1;
        }
    }

    private static void NormalizeSort(IEnumerable<DonorProfileSeed> list)
    {
        var sorted = list.OrderBy(x => x.SortOrder).ToList();
        for (var index = 0; index < sorted.Count; index++)
        {
            sorted[index].SortOrder = index + 1;
        }
    }
}
