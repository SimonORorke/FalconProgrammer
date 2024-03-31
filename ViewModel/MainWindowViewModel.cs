using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ObservableObject {
  [ObservableProperty] private string _currentPageTitle = string.Empty;
  [ObservableProperty] private TabItemViewModel? _selectedTab;
  private ImmutableList<TabItemViewModel>? _tabs;

  internal BatchScriptViewModel BatchScriptViewModel { get; } =
    new BatchScriptViewModel(dialogWrapper, dispatcherService);

  private ViewModelBase? CurrentPageViewModel { get; set; }

  internal GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; } =
    new GuiScriptProcessorViewModel(dialogWrapper, dispatcherService);

  internal LocationsViewModel LocationsViewModel { get; } =
    new LocationsViewModel(dialogWrapper, dispatcherService);

  public ImmutableList<TabItemViewModel> Tabs => _tabs ??= CreateTabs();

  private ImmutableList<TabItemViewModel> CreateTabs() {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(LocationsViewModel),
      new TabItemViewModel(GuiScriptProcessorViewModel),
      new TabItemViewModel(BatchScriptViewModel)
    };
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
      // DispatcherService.Dispatch(()=> SelectedTab = Tabs[1]);
    }
  }
}