# AGENTS.md

## Project goal
Build a production-style Windows desktop application in C# using WPF and MVVM.

This file defines the required scaffold, architecture, build order, and implementation rules for Codex or any coding model working in this repository.

## Core requirements
- Use C# with WPF and MVVM
- Use CommunityToolkit.Mvvm
- Keep business logic out of the UI
- Use a clean layered architecture
- Replace hardcoded lookup values with admin-editable settings
- Use async/await appropriately
- Do not block the UI thread
- Use normal hyphens only, not em dashes
- Prefer maintainable code over clever code
- No pseudocode unless explicitly requested

## App screens
1. Dashboard
2. Calls
3. Supervisor Transfer
4. Review
5. Settings
6. Setup Wizard
7. Tutorial
8. Help
9. Newbie Shift

## Critical behavior rules
- If Call 1 and Call 2 both pass, Call 3 must be hidden
- If "Other" is selected in coaching or fail reasons, a text field must appear and become required
- Review screen must not auto-submit
- Review screen must allow retry and regenerate for summaries
- Settings screen must fully control lookup tables such as shows, call types, donors, supervisor reasons, fail reasons, and coaching categories
- Setup Wizard must configure first-run settings
- Tutorial and Help should be robust and data-driven where reasonable

## Solution structure
```text
AppName.sln
├── AppName.UI
├── AppName.Core
├── AppName.Infrastructure
└── AppName.Tests
```

## AppName.Core
Purpose: business logic, data contracts, rules, validation, state

### Folder structure
```text
AppName.Core
├── Models
│   ├── Enums
│   ├── Config
│   ├── Sessions
│   ├── Lookup
│   ├── Review
│   ├── Help
│   └── Common
├── Interfaces
│   ├── Services
│   ├── Repositories
│   └── Navigation
├── Services
│   ├── Rules
│   ├── Validation
│   ├── State
│   └── Workflow
└── Validators
```

### Required files and responsibilities

#### Models/Enums
- SessionStatus.cs
  - Draft
  - InProgress
  - ReadyForReview
  - Completed
  - Archived
- CallOutcome.cs
  - Pass
  - Fail
  - NotScored
- TransferOutcome.cs
  - Pass
  - Fail
  - NotAttempted
- WarningSeverity.cs
  - Info
  - Warning
  - Error

#### Models/Config
- AppSettings.cs
  - Theme
  - SoundsEnabled
  - DefaultSaveFolder
  - AutosaveMinutes
  - FirstRunComplete
  - TutorialCompleted
  - BeginnerModeEnabled
  - AiSummariesEnabled
  - ActiveAiProvider
  - DatabasePath
  - LogFolderPath
- UserProfile.cs
  - UserName
  - Role
  - LastLogin
  - ShowBeginnerTips
  - RecentSessions

#### Models/Lookup
- LookupItem.cs
  - Guid Id
  - string Category
  - string Name
  - string Description
  - bool IsEnabled
  - int SortOrder
- LookupTableSet.cs
  - Lists for Shows, CallTypes, Donors, SupervisorReasons, FailReasons, CoachingCategories, HelpTopics, TutorialTopics

#### Models/Sessions
- EvaluationSession.cs
  - SessionId
  - EvaluatorName
  - CreatedAt
  - UpdatedAt
  - Status
  - CurrentScreenKey
  - Calls
  - Transfers
  - Review
  - Result
  - Warnings
  - ProgressPercent
- CallRecord.cs
  - CallNumber
  - Outcome
  - CoachingSelections
  - FailSelections
  - OtherCoachingText
  - OtherFailText
  - Notes
  - IsVisible
  - IsComplete
- TransferRecord.cs
  - AttemptNumber
  - Outcome
  - Reason
  - Notes
  - FollowUpRequired
  - FollowUpDate

#### Models/Review
- ReviewData.cs
  - CoachingSummary
  - FailSummary
  - CoachingSummaryGenerated
  - FailSummaryGenerated
  - ReadyForSubmit
  - SubmitConfirmed
- EvaluationResult.cs
  - IsPassing
  - IsComplete
  - ShowCall3
  - RequiresSupervisorFollowUp
  - BlockingIssues
  - InformationalMessages

#### Models/Common
- AppWarning.cs
  - Severity
  - Code
  - Message
  - RelatedSection

#### Models/Help
- TutorialTopic.cs
  - Id
  - Title
  - Category
  - ScreenKey
  - Steps
  - Tags
- TutorialStep.cs
  - StepNumber
  - Title
  - Body
  - Tip
  - ImagePath optional
- HelpArticle.cs
  - Id
  - Title
  - Category
  - Content
  - Tags
  - RelatedScreen

#### Interfaces/Services
- ISettingsService.cs
- IUserProfileService.cs
- ILookupTableService.cs
- IEvaluationRulesService.cs
- IValidationService.cs
- ISessionStateService.cs
- ISummaryService.cs
- IAudioService.cs
- ILogService.cs
- ITutorialContentService.cs
- IHelpContentService.cs

#### Interfaces/Repositories
- ISessionRepository.cs

#### Interfaces/Navigation
- INavigationService.cs

#### Services/Rules
- EvaluationRulesService.cs
  - Determine if Call 3 should show
  - Determine overall pass and fail
  - Determine completion state
  - Determine supervisor follow-up requirement

#### Services/Validation
- ValidationService.cs
  - Validate calls
  - Validate transfers
  - Validate review readiness
  - Validate required Other text fields

#### Services/State
- SessionStateService.cs
  - Hold active session
  - Manage dirty state
  - Update progress
  - Apply rules and warnings

#### Services/Workflow
- WorkflowCoordinator.cs
  - Optional centralized flow coordinator for next and previous screen rules

## AppName.Infrastructure
Purpose: persistence, config, logging, audio, AI, content

### Folder structure
```text
AppName.Infrastructure
├── Configuration
├── Persistence
├── Logging
├── Audio
├── AI
├── Content
└── DependencyInjection
```

### Required files and responsibilities
- Configuration/JsonSettingsService.cs
  - Implements ISettingsService
- Configuration/JsonUserProfileService.cs
  - Implements IUserProfileService
- Configuration/JsonLookupTableService.cs
  - Implements ILookupTableService
- Persistence/DatabaseInitializer.cs
  - Create SQLite database and tables
- Persistence/SqliteSessionRepository.cs
  - Implements ISessionRepository
  - Initial version may store serialized session JSON in SQLite
- Logging/FileLogService.cs
  - Implements ILogService
- Audio/SystemAudioService.cs
  - Implements IAudioService
- AI/FallbackSummaryService.cs
  - Implements ISummaryService without external API dependency
- AI/GeminiSummaryService.cs
  - Optional external summary provider
- AI/SummaryServiceFactory.cs
  - Chooses provider
- Content/JsonTutorialContentService.cs
  - Implements ITutorialContentService
- Content/JsonHelpContentService.cs
  - Implements IHelpContentService
- DependencyInjection/ServiceRegistration.cs
  - Register services, repositories, and ViewModels

## AppName.UI
Purpose: WPF views, viewmodels, controls, styles, navigation

### Folder structure
```text
AppName.UI
├── Views
├── ViewModels
├── Controls
├── Styles
├── Converters
├── Navigation
├── Resources
└── Services
```

### Required files and responsibilities

#### Views
- MainWindow.xaml
  - Hosts the routed content
- DashboardView.xaml
- CallsView.xaml
- SupervisorTransferView.xaml
- ReviewView.xaml
- SettingsView.xaml
- SetupWizardView.xaml
- TutorialView.xaml
- HelpView.xaml
- NewbieShiftView.xaml

#### ViewModels
- ViewModelBase.cs
- MainWindowViewModel.cs
- DashboardViewModel.cs
- CallsViewModel.cs
- CallRecordViewModel.cs
- SupervisorTransferViewModel.cs
- TransferRecordViewModel.cs
- ReviewViewModel.cs
- SettingsViewModel.cs
- SetupWizardViewModel.cs
- TutorialViewModel.cs
- HelpViewModel.cs
- NewbieShiftViewModel.cs

#### Controls
- PassFailToggleControl.xaml
- WarningBannerControl.xaml
- ProgressHeaderControl.xaml
- SectionCardControl.xaml
- OtherTextInputControl.xaml
- LookupTableEditorControl.xaml
- SummaryPanelControl.xaml
- TutorialStepCardControl.xaml
- HelpArticleControl.xaml

#### Styles
- Colors.xaml
- Typography.xaml
- Buttons.xaml
- Inputs.xaml
- Cards.xaml
- Dialogs.xaml
- MergedStyles.xaml

#### Converters
- BoolToVisibilityConverter.cs
- InverseBoolConverter.cs
- EnumToBooleanConverter.cs
- StringNotEmptyToVisibilityConverter.cs

#### Navigation
- NavigationService.cs
- ViewModelFactory.cs
- ViewLocator.cs

#### UI Services
- DialogService.cs

## AppName.Tests
Purpose: test business logic, state, persistence, and ViewModel behavior

### Folder structure
```text
AppName.Tests
├── Rules
├── Validation
├── State
├── Persistence
└── ViewModels
```

### Required test files
- Rules/EvaluationRulesServiceTests.cs
- Validation/ValidationServiceTests.cs
- State/SessionStateServiceTests.cs
- Persistence/JsonSettingsServiceTests.cs
- Persistence/SqliteSessionRepositoryTests.cs
- ViewModels/CallsViewModelTests.cs

## NuGet packages
Install at minimum:
- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Microsoft.Data.Sqlite
- System.Text.Json

Optional later:
- FluentValidation
- MaterialDesignThemes
- MahApps.Metro

## Build order
1. Create solution and projects
2. Add NuGet packages
3. Create Core enums, models, interfaces
4. Create UI ViewModelBase, MainWindow, navigation plumbing
5. Create Infrastructure JSON services
6. Create SessionStateService, EvaluationRulesService, ValidationService
7. Create Dashboard screen
8. Create Calls workflow
9. Create Supervisor Transfer workflow
10. Create Review workflow
11. Create SQLite repository and persistence initialization
12. Create Settings screen and lookup editor
13. Create Setup Wizard
14. Create Tutorial and Help content services and screens
15. Create Newbie Shift screen
16. Add fallback summary service and optional Gemini summary integration
17. Add logging and audio services
18. Add styling and polish
19. Add tests
20. Run audit for missing registrations, namespaces, bindings, and async issues

## Implementation rules
- Keep business rules in Core services, not code-behind
- Keep persistence out of the UI project
- Use reusable controls when a UI pattern repeats
- Prefer complete files over partial snippets
- If a new helper file is required, create it explicitly
- When generating code, list created files at the end
- When auditing code, identify exact file-level fixes

## Recommended Codex workflow
When asked to generate code:
1. Start with foundation only
2. Generate complete files in batches
3. Audit after each batch
4. Fix compile issues before moving on
5. Do not attempt the full app in one shot

## First generation batch
Start with:
- Core enums
- Core models
- Core interfaces
- ViewModelBase
- MainWindow
- MainWindowViewModel
- Navigation service interface and implementation
- DashboardView
- DashboardViewModel
- Dependency injection registration

Do not jump ahead unless explicitly instructed.
