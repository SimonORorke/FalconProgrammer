using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel : SettingsWriterViewModelBase,
  IRecipient<GoToLocationsPageMessage> {
  private ColourSchemeId _colourSchemeId;
  private string _currentPageTitle = string.Empty;

  /// <summary>
  ///   Generates <see cref="SelectedTab" /> property
  ///   and partial OnSelectedTabChanged method.
  /// </summary>
  [ObservableProperty] private TabItemViewModel? _selectedTab;

  private ImmutableList<TabItemViewModel>? _tabs;

  public MainWindowViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService, IWindowLocationService windowLocationService)
    : base(dialogService, dispatcherService) {
    WindowLocationService = windowLocationService;
    BackgroundViewModel = new BackgroundViewModel(dialogService, dispatcherService);
    BatchScriptViewModel = new BatchScriptViewModel(dialogService, dispatcherService);
    GuiScriptProcessorViewModel =
      new GuiScriptProcessorViewModel(dialogService, dispatcherService);
    MidiForMacrosViewModel = new MidiForMacrosViewModel(dialogService, dispatcherService);
    LocationsViewModel = new LocationsViewModel(dialogService, dispatcherService);
    ReverbViewModel = new ReverbViewModel(dialogService, dispatcherService);
    // throw new InvalidOperationException("This is a test exception.");
  }

  public static string ApplicationName => Global.ApplicationName;

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [PublicAPI]
  internal BackgroundViewModel BackgroundViewModel { get; [ExcludeFromCodeCoverage] set; }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [PublicAPI]
  internal BatchScriptViewModel BatchScriptViewModel {
    get;
    [ExcludeFromCodeCoverage] set;
  }

  public ColourSchemeId ColourSchemeId {
    get => _colourSchemeId;
    private set => SetProperty(ref _colourSchemeId, value);
  }

  private ViewModelBase? CurrentPageViewModel { get; set; }

  public string CurrentPageTitle {
    get => _currentPageTitle;
    private set => SetProperty(ref _currentPageTitle, value);
  }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  internal GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; set; }

  private TabItemViewModel LocationsTab => Tabs[1];

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [PublicAPI]
  internal LocationsViewModel LocationsViewModel { get; [ExcludeFromCodeCoverage] set; }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [PublicAPI]
  internal MidiForMacrosViewModel MidiForMacrosViewModel {
    get;
    [ExcludeFromCodeCoverage] set;
  }

  /// <summary>
  ///   Not used because this is not a page but the owner of pages.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public override string PageTitle => string.Empty;

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [PublicAPI]
  internal ReverbViewModel ReverbViewModel { get; [ExcludeFromCodeCoverage] set; }

  public ImmutableList<TabItemViewModel> Tabs => _tabs ??= CreateTabs();

  public IWindowLocationService WindowLocationService { get; }

  public void Receive(GoToLocationsPageMessage message) {
    DispatcherService.Dispatch(() => SelectedTab = LocationsTab);
  }

  /// <summary>
  ///   Generates <see cref="AboutCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task About() {
    await DialogService.ShowAboutBox(CreateAboutViewModel());
  }

  /// <summary>
  ///   Generates <see cref="ManualCommand" />.
  /// </summary>
  [RelayCommand]
  [ExcludeFromCodeCoverage]
  private void Manual() { }

  protected virtual AboutWindowViewModel CreateAboutViewModel() {
    return new AboutWindowViewModel(DialogService);
  }

  private ColourSchemeWindowViewModel CreateColourSchemeWindowViewModel() {
    return new ColourSchemeWindowViewModel(
      ColourSchemeId, DialogService, DispatcherService);
  }

  private ImmutableList<TabItemViewModel> CreateTabs() {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(BatchScriptViewModel),
      new TabItemViewModel(LocationsViewModel),
      new TabItemViewModel(GuiScriptProcessorViewModel),
      new TabItemViewModel(MidiForMacrosViewModel),
      new TabItemViewModel(BackgroundViewModel),
      new TabItemViewModel(ReverbViewModel)
    };
    return list.ToImmutableList();
  }

  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (!IsVisible) {
      try {
        // Start listening for ObservableRecipient messages. Set IsVisible to true.
        Task.Run(async () => await Open()).Wait();
      } catch {
        // Maybe a settings XML error, in which case error messages will have been shown.
      }
    }
    DispatcherService.Dispatch(() => OnSelectedTabChangedAsync(value!));
  }

  private async void OnSelectedTabChangedAsync(TabItemViewModel value) {
    // This method has to be async void rather than async Task because the calling
    // method, OnSelectedTabChanged is a partial method that cannot be a Task.
    if (CurrentPageViewModel != null
        // If a return to the same page has been forced because of errors,
        // the error message that was shown by QueryClose should not be shown again.
        && !CurrentPageViewModel.Equals(value.ViewModel)) {
      // If there is an error on the previous selected tab's page,
      // QueryClose will show an error message box and return false.
      bool canChangeTab = await CurrentPageViewModel.QueryClose();
      if (!canChangeTab) {
        SelectedTab = (
          from tab in Tabs
          where tab.ViewModel == CurrentPageViewModel
          select tab).Single();
        return;
      }
    }
    CurrentPageViewModel = value.ViewModel;
    CurrentPageTitle = CurrentPageViewModel.PageTitle;
    await CurrentPageViewModel.Open();
    // throw new InvalidOperationException("This is a test async void exception.");
  }

  internal override async Task Open() {
    await base.Open();
    ColourSchemeId =
      ColourSchemeWindowViewModel.StringToColourSchemeId(Settings.ColourScheme);
    if (Settings.WindowLocation != null) {
      WindowLocationService.Left = Settings.WindowLocation.Left;
      WindowLocationService.Top = Settings.WindowLocation.Top;
      WindowLocationService.Width = Settings.WindowLocation.Width;
      WindowLocationService.Height = Settings.WindowLocation.Height;
      WindowLocationService.WindowState = Settings.WindowLocation.WindowState;
    }
    foreach (var tab in Tabs) {
      tab.ViewModel.ModelServices = ModelServices;
    }
  }

  public async Task<bool> QueryCloseWindow() {
    // throw new InvalidOperationException("This is a test QueryCloseWindow exception.");
    if (CurrentPageViewModel != null) {
      if (!await CurrentPageViewModel.QueryClose(true)) {
        return false;
      }
    }
    SaveWindowLocationSettingsIfChanged();
    // Stop listening for ObservableRecipient messages. Save settings if changed.
    return await base.QueryClose(true);
  }

  private void SaveWindowLocationSettingsIfChanged() {
    // The window location service settings should have just had its properties updated.
    // But we should check. If the properties have never been restored from saved
    // settings and have never been updated, they should all be null; once updated, none
    // should.
    if (WindowLocationService is {
          Left: not null, Top: not null, Width: not null, Height: not null,
          WindowState: not null
        }) {
      // Window location data is available.
      if (Settings.WindowLocation == null // Never been saved before.
          || Settings.WindowLocation.Left != WindowLocationService.Left.Value
          || Settings.WindowLocation.Top != WindowLocationService.Top.Value 
          || Settings.WindowLocation.Width != WindowLocationService.Width.Value
          || Settings.WindowLocation.Height != WindowLocationService.Height.Value
          || Settings.WindowLocation.WindowState !=
          WindowLocationService.WindowState.Value) {
        // Window location settings have changed or have not previously been saved. 
        // Instantiate Settings.WindowLocation even if it already exists.
        // This will ensure that a settings change will be detected and saved.
        Settings.WindowLocation = new Settings.WindowLocationSettings {
          Left = WindowLocationService.Left.Value,
          Top = WindowLocationService.Top.Value,
          Width = WindowLocationService.Width.Value,
          Height = WindowLocationService.Height.Value,
          WindowState = WindowLocationService.WindowState.Value
        };
      }
    }
  }

  /// <summary>
  ///   Generates <see cref="SelectColourSchemeCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task SelectColourScheme() {
    var colourSchemeWindowViewModel = CreateColourSchemeWindowViewModel();
    await colourSchemeWindowViewModel.Open();
    await DialogService.ShowColourSchemeDialog(colourSchemeWindowViewModel);
    await colourSchemeWindowViewModel.QueryClose();
    ColourSchemeId =
      ColourSchemeWindowViewModel.StringToColourSchemeId(
        colourSchemeWindowViewModel.ColourScheme);
  }
}