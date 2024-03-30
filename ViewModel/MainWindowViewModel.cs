using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(IDialogWrapper dialogWrapper)
  : ViewModelBase(dialogWrapper) {
  [ObservableProperty] private string _currentPageTitle = "Welcome to Avalonia!";
  [ObservableProperty] private TabItemViewModel? _selectedTab;

  public ImmutableList<TabItemViewModel> Tabs { get; } = 
    CreateTabs(dialogWrapper);

  private static ImmutableList<TabItemViewModel> CreateTabs(
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
  
  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (value != null) {
      Console.WriteLine(
        $"MainWindowViewModel.OnSelectedTabChanged: {value.Header}; {value.ViewModel.GetType().Name}");
    }
  }
}