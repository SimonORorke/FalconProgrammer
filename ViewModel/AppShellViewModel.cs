namespace FalconProgrammer.ViewModel;

public class AppShellViewModel(IDialogWrapper dialogWrapper)
  : ViewModelBase(dialogWrapper) {
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