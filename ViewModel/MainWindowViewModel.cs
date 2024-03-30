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

  // public BatchScriptViewModel BatchScriptViewModel { get; } =
  //   new BatchScriptViewModel(dialogWrapper);
  //
  // public GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; } =
  //   new GuiScriptProcessorViewModel(dialogWrapper);
  //
  // public LocationsViewModel LocationsViewModel { get; } =
  //   new LocationsViewModel(dialogWrapper);
  //
  // public void OnSelectedViewModelChanged(ViewModelBase toViewModel) {
  //   Debug.WriteLine(
  //     $"MainWindowViewModel.OnSelectedViewModelChanged: {toViewModel.GetType().Name}");
  // }
}