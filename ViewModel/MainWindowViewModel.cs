using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FalconProgrammer.ViewModel;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public partial class MainWindowViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService),
    IRecipient<GoToLocationsPageMessage> {
  /// <summary>
  ///   Generates CurrentPageTitle property and partial OnCurrentPageTitleChanged method.
  /// </summary>
  [ObservableProperty] private string _currentPageTitle = string.Empty;

  /// <summary>
  ///   Generates SelectedTab property property and partial OnSelectedTabChanged method.
  /// </summary>
  [ObservableProperty] private TabItemViewModel? _selectedTab;

  private ImmutableList<TabItemViewModel>? _tabs;

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal virtual BatchScriptViewModel BatchScriptViewModel { get; set; } =
    new BatchScriptViewModel(dialogWrapper, dispatcherService);

  private ViewModelBase? CurrentPageViewModel { get; set; }

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  internal virtual GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; set; } =
    new GuiScriptProcessorViewModel(dialogWrapper, dispatcherService);

  /// <summary>
  ///   The setter is only for tests.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal virtual LocationsViewModel LocationsViewModel { get; set; } =
    new LocationsViewModel(dialogWrapper, dispatcherService);

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
      new TabItemViewModel(BatchScriptViewModel)
    };
    foreach (var tab in list) {
      tab.ViewModel.ModelServices = ModelServices;
    }
    return list.ToImmutableList();
  }

  public void OnClosing() {
    // TODO: On closing main window, allow cancel close if settings cannot be saved.
    CurrentPageViewModel?.QueryClose();
    QueryClose();
  }

  partial void OnSelectedTabChanged(TabItemViewModel? value) {
    if (value != null) {
      if (!IsVisible) {
        // Start listening for ObservableRecipient messages. Set IsVisible to true.  
        Open();
      }
      if (CurrentPageViewModel != null
          // If a return to the same page has been forced because of errors,
          // the error message that was shown by QueryClose should not be shown again.
          && !CurrentPageViewModel.Equals(value.ViewModel)
          // If there is an errors on the previous selected tab's page,
          // QueryClose will show a error message box and return false.
          && !CurrentPageViewModel.QueryClose()) {
        // Go back to the previous tab.
        DispatcherService.Dispatch(() =>
          SelectedTab = (
            from tab in Tabs
            where tab.ViewModel == CurrentPageViewModel
            select tab).Single());
        return;
      }
      CurrentPageViewModel = value.ViewModel;
      CurrentPageTitle = CurrentPageViewModel.PageTitle;
      CurrentPageViewModel.Open();
    }
  }
}