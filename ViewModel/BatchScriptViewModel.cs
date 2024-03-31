namespace FalconProgrammer.ViewModel;

public class BatchScriptViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogWrapper, dispatcherService) {
  public override string PageTitle => "Batch Script";
}