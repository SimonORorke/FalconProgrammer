using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class TestMainWindowViewModel : MainWindowViewModel {
  public TestMainWindowViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService, IWindowLocationService windowLocationService) 
    : base(dialogService, dispatcherService, windowLocationService) { }

  protected override AboutWindowViewModel CreateAboutViewModel() {
    var result = base.CreateAboutViewModel();
    result.ApplicationInfo = new MockApplicationInfo();
    return result;
  }
}