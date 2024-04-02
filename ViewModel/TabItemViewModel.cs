namespace FalconProgrammer.ViewModel;

public class TabItemViewModel(ViewModelBase viewModel) {
  public string Header { get; } = viewModel.TabTitle;
  public ViewModelBase ViewModel { get; } = viewModel;
}