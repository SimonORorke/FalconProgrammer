using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class AppShellViewModel() : ViewModelBase(Global.ApplicationTitle) {
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
    // Debug.WriteLine($"AppShellViewModel.OnNavigated: {CurrentPageTitle}");
  }
}