using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public partial class MainWindowViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogService, dispatcherService),
    IRecipient<GoToLocationsPageMessage> {
  /// <summary>
  ///   Generates CurrentPageTitle property.
  /// </summary>
  [ObservableProperty] private string _currentPageTitle = string.Empty;

  /// <summary>
  ///   Generates SelectedTab property and partial OnSelectedTabChanged method.
  /// </summary>
  [ObservableProperty] private TabItemViewModel? _selectedTab;

  private ImmutableList<TabItemViewModel>? _tabs;

#pragma warning disable CA1822
  // ReSharper disable once MemberCanBeMadeStatic.Global
  // Notwithstanding the warnings from both ReSharper and the compiler that this property
  // should be static, App.OnFrameworkInitializationCompleted cannot access it if it is
  // static: "Cannot access static property 'ApplicationTitle' in non-static context".
  public string ApplicationTitle {
#pragma warning restore CA1822
    get => Global.ApplicationTitle;
    set => Global.ApplicationTitle = value;
  }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal virtual BatchScriptViewModel BatchScriptViewModel { get; set; } =
    new BatchScriptViewModel(dialogService, dispatcherService);

  private ViewModelBase? CurrentPageViewModel { get; set; }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  internal virtual GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; set; } =
    new GuiScriptProcessorViewModel(dialogService, dispatcherService);

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal virtual MidiForMacrosViewModel MidiForMacrosViewModel { get; set; } =
    new MidiForMacrosViewModel(dialogService, dispatcherService);

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal virtual LocationsViewModel LocationsViewModel { get; set; } =
    new LocationsViewModel(dialogService, dispatcherService);

  /// <summary>
  ///   Not used because this is not a page but the owner of pages.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public override string PageTitle => throw new NotSupportedException();

  public ImmutableList<TabItemViewModel> Tabs => _tabs ??= CreateTabs();

  public void Receive(GoToLocationsPageMessage message) {
    DispatcherService.Dispatch(() => SelectedTab = Tabs[0]);
  }

  private ImmutableList<TabItemViewModel> CreateTabs() {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(LocationsViewModel),
      new TabItemViewModel(GuiScriptProcessorViewModel),
      new TabItemViewModel(MidiForMacrosViewModel),
      new TabItemViewModel(BatchScriptViewModel)
    };
    // foreach (var tab in list) {
    //   tab.ViewModel.ModelServices = ModelServices;
    // }
    return list.ToImmutableList();
  }

  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (value == null) {
      return;
    }
    if (!IsVisible) {
      // Start listening for ObservableRecipient messages. Set IsVisible to true.
      Task.Run(async ()=>await Open()).Wait();
    }
    DispatcherService.Dispatch(() => OnSelectedTabChangedAsync(value));
  }

  private async void OnSelectedTabChangedAsync(TabItemViewModel value) {
    if (CurrentPageViewModel != null
        // If a return to the same page has been forced because of errors,
        // the error message that was shown by QueryCloseAsync should not be shown again.
        && !CurrentPageViewModel.Equals(value.ViewModel)) {
      // If there is an error on the previous selected tab's page,
      // QueryCloseAsync will show an error message box and return false.
      bool canChangeTab = await CurrentPageViewModel.QueryCloseAsync();
      if (!canChangeTab) {
        // Go back to the previous tab.
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
  }

  internal override async Task Open() {
    await base.Open();
    foreach (var tab in Tabs) {
      tab.ViewModel.ModelServices = ModelServices;
    }
  }

  public async Task<bool> QueryCloseWindowAsync() {
    if (CurrentPageViewModel != null) {
      if (!await CurrentPageViewModel.QueryCloseAsync(true)) {
        return false;
      }
    }
    // // Stop listening for ObservableRecipient messages.
    // Messenger.UnregisterAll(this);
    return await base.QueryCloseAsync(true);
  }
}