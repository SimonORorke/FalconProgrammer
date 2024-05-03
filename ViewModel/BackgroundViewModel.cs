namespace FalconProgrammer.ViewModel;

public class BackgroundViewModel : SettingsWriterViewModelBase {
  public BackgroundViewModel(IDialogService dialogService, 
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }
  
  public override string PageTitle => "Background Image for Info Page";
  public override string TabTitle => "Background";

}