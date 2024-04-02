using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MainWindowViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    TestGuiScriptProcessorViewModel = new TestGuiScriptProcessorViewModel(
      MockDialogWrapper, MockDispatcherService);
    ViewModel = new MainWindowViewModel(
      MockDialogWrapper, MockDispatcherService) {
      ModelServices = TestModelServices,
      GuiScriptProcessorViewModel = TestGuiScriptProcessorViewModel
    };
  }

  private TestGuiScriptProcessorViewModel TestGuiScriptProcessorViewModel { get; set; } =
    null!;

  private MainWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void Main() {
    // Access tabs to emulate the initial display of the main window.
    // That will cause the tabs to be created and their associated page view models are
    // configured correctly.  In particular, the page view models will get the default
    // settings from an embedded resource file rather than reading the real settings file
    // from the file system.
    Assert.That(ViewModel.Tabs[0].Header,
      Is.EqualTo(ViewModel.LocationsViewModel.TabTitle));
    Assert.That(ViewModel.Tabs[2].Header,
      Is.EqualTo(ViewModel.BatchScriptViewModel.TabTitle));
    ViewModel.SelectedTab = ViewModel.Tabs[0]; // Locations view model 
    // Skip the GUI Script Processor page's initial validation.
    // Otherwise the validation might fail, resulting in the page immediately being
    // replaced with the Locations page.
    TestGuiScriptProcessorViewModel.SkipInitialisation = true;
    var selectedPageViewModel = TestGuiScriptProcessorViewModel;
    ViewModel.SelectedTab = ViewModel.Tabs[1]; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.Header, Is.EqualTo(selectedPageViewModel.TabTitle));
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(selectedPageViewModel.PageTitle));
    ViewModel.OnClosing();
    Assert.That(selectedPageViewModel.ClosedCount, Is.EqualTo(1));
  }

  [Test]
  public void GoToLocationsPage() {
    // Access at least one tab to emulate the initial display of the main window.
    // That will cause the tabs to be created and their associated page view models are
    // configured correctly.  In particular, the page view models will get the default
    // settings from an embedded resource file rather than reading the real settings file
    // from the file system.
    ViewModel.SelectedTab = ViewModel.Tabs[0]; // Locations view model 
    // The GUI Script Processor page's initial validation will fail, resulting in the
    // page immediately being replaced with the Locations page.
    ViewModel.SelectedTab = ViewModel.Tabs[1]; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    Assert.That(MockDispatcherService.DispatchCount, Is.EqualTo(1));
  }
}