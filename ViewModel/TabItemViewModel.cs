namespace FalconProgrammer.ViewModel;

/// <summary>
///   View model for a tab page, providing the tab header title and the view model for
///   the page content.
/// </summary>
/// <remarks>
///   Not to be renamed to TabItem, as we need to access the TabItem control in view code.
/// </remarks>
public class TabItemViewModel {
  public TabItemViewModel(ViewModelBase viewModel) {
    Header = viewModel.TabTitle;
    ViewModel = viewModel;
  }

  /// <summary>
  ///   Gets the tab header title.
  /// </summary>
  public string Header { get; }
  
  /// <summary>
  ///   Gets the view model for the page content. 
  /// </summary>
  public ViewModelBase ViewModel { get; }
}