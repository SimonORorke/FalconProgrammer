﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public partial class MainWindowViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService), INavigator {
  [ObservableProperty] private string _currentPageTitle = string.Empty;
  [ObservableProperty] private TabItemViewModel? _selectedTab;
  private ImmutableList<TabItemViewModel>? _tabs;

  internal virtual BatchScriptViewModel BatchScriptViewModel { get; } =
    new BatchScriptViewModel(dialogWrapper, dispatcherService);

  private ViewModelBase? CurrentPageViewModel { get; set; }

  internal virtual GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; } =
    new GuiScriptProcessorViewModel(dialogWrapper, dispatcherService);

  internal virtual LocationsViewModel LocationsViewModel { get; } =
    new LocationsViewModel(dialogWrapper, dispatcherService);

  /// <summary>
  ///   Not used because this is not a page but the owner of pages.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public override string PageTitle => throw new NotSupportedException();

  public ImmutableList<TabItemViewModel> Tabs => _tabs ??= CreateTabs();

  void INavigator.GoToLocationsPage() {
    DispatcherService.Dispatch(() => SelectedTab = Tabs[0]);
  }

  private ImmutableList<TabItemViewModel> CreateTabs() {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(LocationsViewModel),
      new TabItemViewModel(GuiScriptProcessorViewModel),
      new TabItemViewModel(BatchScriptViewModel)
    };
    foreach (var tab in list) {
      tab.ViewModel.Navigator = this;
    }
    return list.ToImmutableList();
  }

  public void OnClosing() {
    CurrentPageViewModel?.OnDisappearing();
  }

  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (value != null) {
      CurrentPageViewModel?.OnDisappearing();
      CurrentPageViewModel = value.ViewModel;
      CurrentPageTitle = CurrentPageViewModel.PageTitle;
      CurrentPageViewModel.OnAppearing();
    }
  }
}