using System.Diagnostics;

namespace FalconProgrammer.ViewModel;

public class AppShellViewModel() : ViewModelBase("Shell") {
  private string _currentPageTitle = string.Empty;

  public string CurrentPageTitle {
    get => _currentPageTitle;
    set {
      _currentPageTitle = value;
      OnPropertyChanged();
    }
  }
  
  public void OnNavigated() {
    CurrentPageTitle = ServiceHelper.CurrentPageTitle;
    Debug.WriteLine($"{GetType().Name}.OnNavigated: {CurrentPageTitle}");
    // CurrentPageTitle = $"{DateTime.Now}";
    // Debug.WriteLine("OnNavigated");
  }
}