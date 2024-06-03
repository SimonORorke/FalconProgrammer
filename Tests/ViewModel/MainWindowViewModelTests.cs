using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MainWindowViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    MockWindowLocationService = new MockWindowLocationService();
    TestBatchScriptViewModel = new TestBatchScriptViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    TestGuiScriptProcessorViewModel = new TestGuiScriptProcessorViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    ViewModel = new TestMainWindowViewModel(
      MockDialogService, MockDispatcherService, MockWindowLocationService) {
      ModelServices = TestModelServices,
      BatchScriptViewModel = TestBatchScriptViewModel,
      GuiScriptProcessorViewModel = TestGuiScriptProcessorViewModel
    };
  }

  private TabItemViewModel BatchScriptTab => ViewModel.Tabs[0];
  private TabItemViewModel GuiScriptProcessorTab => ViewModel.Tabs[2];
  private TabItemViewModel LocationsTab => ViewModel.Tabs[1];
  private TabItemViewModel MidiForMacrosTab => ViewModel.Tabs[3];
  private MockWindowLocationService MockWindowLocationService { get; set; } = null!;
  private TestBatchScriptViewModel TestBatchScriptViewModel { get; set; } = null!;

  private TestGuiScriptProcessorViewModel TestGuiScriptProcessorViewModel { get; set; } =
    null!;

  private TestMainWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void ApplicationName() {
    Global.ApplicationName = "Test Name";
    Assert.That(MainWindowViewModel.ApplicationName, Is.EqualTo(Global.ApplicationName));
  }

  [Test]
  public async Task ColourSchemeInvalid() {
    MockSettingsReaderEmbedded.EmbeddedFileName = "InvalidColourSchemeSettings.xml";
    await ViewModel.Open();
    Assert.That(ViewModel.ColourSchemeId, Is.EqualTo(ColourSchemeId.Lavender));
  }

  [Test]
  public async Task ColourSchemeNotFound() {
    MockSettingsReaderEmbedded.EmbeddedFileName = "LocationsSettings.xml";
    await ViewModel.Open();
    Assert.That(ViewModel.ColourSchemeId, Is.EqualTo(ColourSchemeId.Lavender));
  }

  [Test]
  public void DisallowChangePage() {
    // Show Locations tab initially.
    ViewModel.SelectedTab = LocationsTab;
    // Unspecified settings folder is an error condition that should force the user to
    // stay on the Locations page.
    ViewModel.LocationsViewModel.SettingsFolderPath = string.Empty;
    // Try going to another page.
    ViewModel.SelectedTab = GuiScriptProcessorTab; // Test GUI Script Processor view model
    // An error message box should have been shown and the Locations page should still be
    // shown.
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    Assert.That(ViewModel.SelectedTab, Is.SameAs(LocationsTab));
  }

  [Test]
  public async Task Main() {
    // Show Locations tab initially.
    ViewModel.SelectedTab = LocationsTab;
    Assert.That(BatchScriptTab.Header,
      Is.EqualTo(ViewModel.BatchScriptViewModel.TabTitle));
    Assert.That(LocationsTab.Header,
      Is.EqualTo(ViewModel.LocationsViewModel.TabTitle));
    Assert.That(GuiScriptProcessorTab.Header,
      Is.EqualTo(ViewModel.GuiScriptProcessorViewModel.TabTitle));
    Assert.That(MidiForMacrosTab.Header,
      Is.EqualTo(ViewModel.MidiForMacrosViewModel.TabTitle));
    // Skip the GUI Script Processor page's initial validation.
    // Otherwise the validation might fail, resulting in the page immediately being
    // replaced with the Locations page.
    TestGuiScriptProcessorViewModel.SkipInitialisation = true;
    var selectedPageViewModel = TestGuiScriptProcessorViewModel;
    ViewModel.SelectedTab =
      GuiScriptProcessorTab; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.Header, Is.EqualTo(selectedPageViewModel.TabTitle));
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(selectedPageViewModel.PageTitle));
    await ViewModel.QueryCloseWindow();
    Assert.That(selectedPageViewModel.ClosedCount, Is.EqualTo(1));
  }

  [Test]
  public void GoToLocationsPage() {
    // Show Locations tab initially.
    ViewModel.SelectedTab = LocationsTab;
    // The GUI Script Processor page's initial validation will fail, resulting in the
    // page immediately being replaced with the Locations page.
    ViewModel.SelectedTab =
      GuiScriptProcessorTab; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    Assert.That(ViewModel.SelectedTab, Is.SameAs(LocationsTab));
  }

  [Test]
  public void SettingsXmlError() {
    MockSettingsFolderLocationReader.EmbeddedFileName =
      "InvalidXmlSettings.xml";
    ViewModel.SelectedTab = LocationsTab; // Locations
    Assert.That(ViewModel.CurrentPageTitle,
      Is.EqualTo(ViewModel.LocationsViewModel.PageTitle));
  }

  [Test]
  public async Task ShowAboutBox() {
    await ViewModel.AboutCommand.ExecuteAsync(null);
    Assert.That(MockDialogService.ShowAboutBoxCount, Is.EqualTo(1));
  }

  [Test]
  public async Task ShowColourSchemeDialog() {
    await ViewModel.SelectColourSchemeCommand.ExecuteAsync(null);
    Assert.That(MockDialogService.ShowColourSchemeDialogCount, Is.EqualTo(1));
  }

  [Test]
  public async Task UserConfirmsCloseWindowWhenError() {
    ViewModel.SelectedTab = LocationsTab; // Locations
    // Error condition: settings folder not found.
    // So the user should be prompted to confirm closing the window.
    MockFileSystemService.Folder.SimulatedExists = false;
    // User will confirm that the window should be closed, even though there is a error.
    MockDialogService.SimulatedYesNoAnswer = true;
    // Make a property change to require saving settings.
    ViewModel.LocationsViewModel.ProgramsFolderPath += "X";
    bool canClose = await ViewModel.QueryCloseWindow();
    // Question message box shown.
    // In this test, there are two mock message box questions, the first from
    // LocationViewModel, the second from MainWindowViewModel. In the application, only
    // the first message box question is shown, as the user had to respond to it, to
    // either confirm or cancel the window closure.
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.GreaterThanOrEqualTo(1));
    Assert.That(canClose, Is.True);
  }

  [Test]
  public async Task UserCancelsCloseWindowWhenError() {
    ViewModel.SelectedTab = LocationsTab; // Locations
    // Error condition: settings folder not found.
    // So the user should be prompted to confirm closing the window.
    MockFileSystemService.Folder.SimulatedExists = false;
    // User will decline to close the window, so that the error can be fixed.
    MockDialogService.SimulatedYesNoAnswer = false;
    // Make a property change to require saving settings.
    ViewModel.LocationsViewModel.ProgramsFolderPath += "X";
    bool canClose = await ViewModel.QueryCloseWindow();
    // Question message box shown.
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.EqualTo(1));
    Assert.That(canClose, Is.False);
  }

  [Test]
  public async Task WindowLocationNotPreviouslySaved() {
    ViewModel.SelectedTab = BatchScriptTab;
    Assert.That(ViewModel.Settings.WindowLocation, Is.Null);
    Assert.That(ViewModel.WindowLocationService.Left, Is.Null);
    Assert.That(ViewModel.WindowLocationService.Top, Is.Null);
    Assert.That(ViewModel.WindowLocationService.Width, Is.Null);
    Assert.That(ViewModel.WindowLocationService.Height, Is.Null);
    Assert.That(ViewModel.WindowLocationService.WindowState, Is.Null);
    const int left = 10;
    const int top = 20;
    const int width = 600;
    const int height = 800;
    const int windowState = 2;
    ViewModel.WindowLocationService.Left = left;
    ViewModel.WindowLocationService.Top = top;
    ViewModel.WindowLocationService.Width = width;
    ViewModel.WindowLocationService.Height = height;
    ViewModel.WindowLocationService.WindowState = windowState;
    await ViewModel.QueryCloseWindow();
    Assert.That(ViewModel.Settings.WindowLocation, Is.Not.Null);
    Assert.That(ViewModel.Settings.WindowLocation.Left, Is.EqualTo(left));
    Assert.That(ViewModel.Settings.WindowLocation.Top, Is.EqualTo(top));
    Assert.That(ViewModel.Settings.WindowLocation.Width, Is.EqualTo(width));
    Assert.That(ViewModel.Settings.WindowLocation.Height, Is.EqualTo(height));
    Assert.That(ViewModel.Settings.WindowLocation.WindowState, Is.EqualTo(windowState));
  }

  [Test]
  public async Task WindowLocationPreviouslySaved() {
    var settings = ReadMockSettings("BatchSettings.xml");
    TestBatchScriptViewModel.ConfigureValidMockFileSystemService(settings);
    ViewModel.SelectedTab = BatchScriptTab;
    Assert.That(ViewModel.Settings.WindowLocation, Is.Not.Null);
    Assert.That(ViewModel.WindowLocationService.Left,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Left));
    Assert.That(ViewModel.WindowLocationService.Left, Is.EqualTo(248));
    Assert.That(ViewModel.WindowLocationService.Top,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Top));
    Assert.That(ViewModel.WindowLocationService.Width,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Width));
    Assert.That(ViewModel.WindowLocationService.Height,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Height));
    Assert.That(ViewModel.WindowLocationService.WindowState,
      Is.EqualTo(ViewModel.Settings.WindowLocation.WindowState));
    Assert.That(ViewModel.BatchScriptViewModel.Scope.SoundBank, Is.EqualTo("All"));
    const int left = 137;
    const string soundBank = "Factory";
    ViewModel.WindowLocationService.Left = left;
    ViewModel.BatchScriptViewModel.Scope.SoundBank = soundBank; 
    // Check that changes to page view model property and main window view model property
    // are both saved to settings when the window is closed.
    await ViewModel.QueryCloseWindow();
    Assert.That(ViewModel.Settings.WindowLocation.Left, Is.EqualTo(left));
    Assert.That(ViewModel.Settings.Batch.Scope.SoundBank, Is.EqualTo(soundBank));
  }
}