using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(IDialogWrapper dialogWrapper)
  : ViewModelBase(dialogWrapper) {
  [ObservableProperty] private string _currentPageTitle = "Welcome to Avalonia!";

  public ImmutableList<TabItemViewModel> TabItemViewModels { get; } = 
    CreateTabItemModels(dialogWrapper);

  private static ImmutableList<TabItemViewModel> CreateTabItemModels(
    IDialogWrapper dialogWrapper) {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(
        "Locations", new LocationsViewModel(dialogWrapper)),
      new TabItemViewModel(
        "Script Processor", new GuiScriptProcessorViewModel(dialogWrapper)),
      new TabItemViewModel(
        "Batch Script", new BatchScriptViewModel(dialogWrapper))
    };
    return list.ToImmutableList();
  }

  public void OnSelectedTabChanged(TabItemViewModel tab) {
    Console.WriteLine(
      $"MainWindowViewModel.OnSelectedTabChanged: {tab.Header}; {tab.ViewModel.GetType().Name}");
  }
}