using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService) {
  [ObservableProperty] private string _currentPageTitle = "Welcome to Avalonia!";
  [ObservableProperty] private TabItemViewModel? _selectedTab;

  public ImmutableList<TabItemViewModel> Tabs { get; } =
    CreateTabs(dialogWrapper, dispatcherService);

  private static ImmutableList<TabItemViewModel> CreateTabs(
    IDialogWrapper dialogWrapper,
    IDispatcherService dispatcherService) {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(
        "Locations", new LocationsViewModel(dialogWrapper, dispatcherService)),
      new TabItemViewModel(
        "Script Processor",
        new GuiScriptProcessorViewModel(dialogWrapper, dispatcherService)),
      new TabItemViewModel(
        "Batch Script", new BatchScriptViewModel(dialogWrapper, dispatcherService))
    };
    return list.ToImmutableList();
  }

  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (value != null) {
      Console.WriteLine(
        $"MainWindowViewModel.OnSelectedTabChanged: {value.Header}; {value.ViewModel.GetType().Name}");
      // DispatcherService.Dispatch(()=>SelectedTab = Tabs[1]);
    }
  }
}