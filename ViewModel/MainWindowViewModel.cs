using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MainWindowViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService) {
  
  private ImmutableList<TabItemViewModel>? _tabs;
  
  [ObservableProperty] private string _currentPageTitle = string.Empty;
  [ObservableProperty] private TabItemViewModel? _selectedTab;

  internal BatchScriptViewModel BatchScriptViewModel { get; } =
    new BatchScriptViewModel(dialogWrapper, dispatcherService); 

  internal GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; } =
    new GuiScriptProcessorViewModel(dialogWrapper, dispatcherService); 

  internal LocationsViewModel LocationsViewModel { get; } =
    new LocationsViewModel(dialogWrapper, dispatcherService); 

  public ImmutableList<TabItemViewModel> Tabs => _tabs ??= CreateTabs();

  /// <summary>
  ///   Not required, as the title of the current tab page will be shown.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public override string PageTitle => string.Empty;
  
  private ImmutableList<TabItemViewModel> CreateTabs() {
    var list = new List<TabItemViewModel> {
      new TabItemViewModel(LocationsViewModel),
      new TabItemViewModel(GuiScriptProcessorViewModel),
      new TabItemViewModel(BatchScriptViewModel)
    };
    return list.ToImmutableList();
  }

  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (value != null) {
      CurrentPageTitle = value.ViewModel.PageTitle;
      // Console.WriteLine(
      //   $"MainWindowViewModel.OnSelectedTabChanged: {value.Header}; {value.ViewModel.GetType().Name}");
      // DispatcherService.Dispatch(()=>SelectedTab = Tabs[1]);
    }
  }
}