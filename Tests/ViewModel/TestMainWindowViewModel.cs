using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class TestMainWindowViewModel : MainWindowViewModel {
  public TestMainWindowViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService, ICursorService cursorService,
    IWindowLocationService windowLocationService)
    : base(dialogService, dispatcherService, cursorService, windowLocationService) { }

  internal ColourSchemeId SimulatedNewColourSchemeId { get; set; }
  internal bool? SimulatedNewAutoCheckNewVersions { get; set; }
  internal string? SimulatedNewIgnoreVersion { get; set; }

  protected override AboutWindowViewModel CreateAboutViewModel() {
    var result = base.CreateAboutViewModel();
    result.ApplicationInfo = new MockApplicationInfo();
    return result;
  }

  protected override ColourSchemeWindowViewModel CreateColourSchemeWindowViewModel() {
    var result = base.CreateColourSchemeWindowViewModel();
    result.ColourScheme = SimulatedNewColourSchemeId.ToString();
    return result;
  }

  protected override NewVersionWindowViewModel CreateNewVersionWindowViewModel(
    string newVersion) {
    var result = base.CreateNewVersionWindowViewModel(newVersion);
    result.ApplicationInfo = new MockApplicationInfo();
    if (SimulatedNewAutoCheckNewVersions.HasValue) {
      result.AutoCheckNewVersions = SimulatedNewAutoCheckNewVersions.Value;
    }
    if (SimulatedNewIgnoreVersion != null) {
      result.IgnoreVersion = SimulatedNewIgnoreVersion;
    }
    return result;
  }
}