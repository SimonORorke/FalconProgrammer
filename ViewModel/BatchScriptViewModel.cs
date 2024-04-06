namespace FalconProgrammer.ViewModel;

public class BatchScriptViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  public override string PageTitle => "Batch Script";
}