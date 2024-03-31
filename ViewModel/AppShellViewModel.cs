namespace FalconProgrammer.ViewModel;

public class AppShellViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : ViewModelBase(dialogWrapper, dispatcherService) {
  private string _currentPageTitle = string.Empty;

  public string CurrentPageTitle {
    get => _currentPageTitle;
    private set {
      _currentPageTitle = value;
      OnPropertyChanged();
    }
  }

  public void OnNavigated() {
    CurrentPageTitle = ServiceHelper.CurrentPageTitle;
  }
}