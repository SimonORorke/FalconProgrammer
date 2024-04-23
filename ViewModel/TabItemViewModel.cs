namespace FalconProgrammer.ViewModel;

public class TabItemViewModel {
  public TabItemViewModel(ViewModelBase viewModel) {
    Header = viewModel.TabTitle;
    ViewModel = viewModel;
  }

  public string Header { get; }
  public ViewModelBase ViewModel { get; }
}