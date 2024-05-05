﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel : ViewModelBase,
  IRecipient<GoToLocationsPageMessage> {
  /// <summary>
  ///   Generates CurrentPageTitle property.
  /// </summary>
  [ObservableProperty] private string _currentPageTitle = string.Empty;

  /// <summary>
  ///   Generates SelectedTab property and partial OnSelectedTabChanged method.
  /// </summary>
  [ObservableProperty] private TabItem? _selectedTab;

  private ImmutableList<TabItem>? _tabs;

  public MainWindowViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) {
    BatchScriptViewModel = new BatchScriptViewModel(dialogService, dispatcherService);
    GuiScriptProcessorViewModel =
      new GuiScriptProcessorViewModel(dialogService, dispatcherService);
    MidiForMacrosViewModel = new MidiForMacrosViewModel(dialogService, dispatcherService);
    LocationsViewModel = new LocationsViewModel(dialogService, dispatcherService);
  }

#pragma warning disable CA1822
  // ReSharper disable once MemberCanBeMadeStatic.Global
  // Notwithstanding the warnings from both ReSharper and the compiler that this property
  // should be static, App.OnFrameworkInitializationCompleted cannot access it if it is
  // static: "Cannot access static property 'ApplicationName' in non-static context".
  public string ApplicationTitle {
#pragma warning restore CA1822
    get => Global.ApplicationName;
    set => Global.ApplicationName = value;
  }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal BatchScriptViewModel BatchScriptViewModel { get; set; }

  private ViewModelBase? CurrentPageViewModel { get; set; }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  internal GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; set; }

  private TabItem LocationsTab => Tabs[1];

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal LocationsViewModel LocationsViewModel { get; set; }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal MidiForMacrosViewModel MidiForMacrosViewModel { get; set; }

  /// <summary>
  ///   Not used because this is not a page but the owner of pages.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public override string PageTitle => throw new NotSupportedException();

  public ImmutableList<TabItem> Tabs => _tabs ??= CreateTabs();

  public void Receive(GoToLocationsPageMessage message) {
    DispatcherService.Dispatch(() => SelectedTab = LocationsTab);
  }

  private ImmutableList<TabItem> CreateTabs() {
    var list = new List<TabItem> {
      new TabItem(BatchScriptViewModel),
      new TabItem(LocationsViewModel),
      new TabItem(GuiScriptProcessorViewModel),
      new TabItem(MidiForMacrosViewModel)
    };
    return list.ToImmutableList();
  }

  partial void OnSelectedTabChanged(TabItem? value) {
    if (value == null) {
      return;
    }
    if (!IsVisible) {
      // Start listening for ObservableRecipient messages. Set IsVisible to true.
      Task.Run(async () => await Open()).Wait();
    }
    DispatcherService.Dispatch(() => OnSelectedTabChangedAsync(value));
  }

  private async void OnSelectedTabChangedAsync(TabItem value) {
    if (CurrentPageViewModel != null
        // If a return to the same page has been forced because of errors,
        // the error message that was shown by QueryClose should not be shown again.
        && !CurrentPageViewModel.Equals(value.ViewModel)) {
      // If there is an error on the previous selected tab's page,
      // QueryClose will show an error message box and return false.
      bool canChangeTab = await CurrentPageViewModel.QueryClose();
      if (!canChangeTab) {
        // Go to the Locations page.
        SelectedTab = (
          from tab in Tabs
          where tab.ViewModel == LocationsViewModel
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

  public async Task<bool> QueryCloseWindow() {
    if (CurrentPageViewModel != null) {
      if (!await CurrentPageViewModel.QueryClose(true)) {
        return false;
      }
    }
    // // Stop listening for ObservableRecipient messages.
    return await base.QueryClose(true);
  }
}