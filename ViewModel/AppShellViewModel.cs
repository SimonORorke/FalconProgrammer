namespace FalconProgrammer.ViewModel;

public class AppShellViewModel : ViewModelBase {
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