namespace FalconProgrammer.ViewModel;

public class TabItemViewModel(string header, ViewModelBase viewModel) {
  public string Header { get; } = header;
  public ViewModelBase ViewModel { get; } = viewModel;
}
