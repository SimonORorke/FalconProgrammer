namespace FalconProgrammer.ViewModel;

public class BatchScriptViewModel : SettingsWriterViewModelBase {
  public BatchScriptViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  public override string PageTitle => "Batch Script";
}