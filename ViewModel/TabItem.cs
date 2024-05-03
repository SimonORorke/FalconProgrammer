namespace FalconProgrammer.ViewModel;

public class TabItem {
  public TabItem(ViewModelBase viewModel) {
    Header = viewModel.TabTitle;
    ViewModel = viewModel;
  }

  public string Header { get; }
  public ViewModelBase ViewModel { get; }
}