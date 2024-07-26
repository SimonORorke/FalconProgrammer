using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MainWindowViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    Settings = ReadMockSettings(MockSettingsReaderEmbedded.EmbeddedFileName);
    MockCursorService = new MockCursorService();
    MockWindowLocationService = new MockWindowLocationService();
    TestBatchViewModel = new TestBatchViewModel(
      MockDialogService, MockDispatcherService, MockCursorService) {
      ModelServices = TestModelServices
    };
    TestGuiScriptProcessorViewModel = new TestGuiScriptProcessorViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    ViewModel = new TestMainWindowViewModel(
      MockDialogService, MockDispatcherService, MockCursorService,
      MockWindowLocationService) {
      ModelServices = TestModelServices,
      BatchViewModel = TestBatchViewModel,
      GuiScriptProcessorViewModel = TestGuiScriptProcessorViewModel
    };
  }

  private TabItemViewModel BatchScriptTab => ViewModel.Tabs[0];
  private TabItemViewModel GuiScriptProcessorTab => ViewModel.Tabs[2];
  private TabItemViewModel LocationsTab => ViewModel.Tabs[1];
  private TabItemViewModel MidiForMacrosTab => ViewModel.Tabs[3];
  private MockCursorService MockCursorService { get; set; } = null!;
  private MockWindowLocationService MockWindowLocationService { get; set; } = null!;
  private Settings Settings { get; set; } = null!;
  private TestBatchViewModel TestBatchViewModel { get; set; } = null!;

  private TestGuiScriptProcessorViewModel TestGuiScriptProcessorViewModel { get; set; } =
    null!;

  private TestMainWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void ApplicationName() {
    Global.ApplicationName = "Test Name";
    Assert.That(MainWindowViewModel.ApplicationName, Is.EqualTo(Global.ApplicationName));
  }

  [Test]
  public async Task ChangePropertyValues() {
    var settings = ReadMockSettings("BatchSettings.xml");
    TestBatchViewModel.ConfigureValidMockFileSystemService(settings);
    ViewModel.SelectedTab = BatchScriptTab;
    Assert.That(ViewModel.Settings.WindowLocation, Is.Not.Null);
    Assert.That(ViewModel.WindowLocationService.Left,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Left));
    Assert.That(ViewModel.WindowLocationService.Top,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Top));
    Assert.That(ViewModel.WindowLocationService.Width,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Width));
    Assert.That(ViewModel.WindowLocationService.Height,
      Is.EqualTo(ViewModel.Settings.WindowLocation.Height));
    Assert.That(ViewModel.WindowLocationService.WindowState,
      Is.EqualTo(ViewModel.Settings.WindowLocation.WindowState));
    // Check that changes to page view model property and main window view model
    // properties are all saved to settings when the window is closed.
    Assert.That(ViewModel.ColourSchemeId, Is.EqualTo(ColourSchemeId.Forest));
    Assert.That(ViewModel.WindowLocationService.Left, Is.EqualTo(248));
    Assert.That(ViewModel.BatchViewModel.Scope.SoundBank, Is.EqualTo("All"));
    const ColourSchemeId colourSchemeId = ColourSchemeId.Lavender;
    const int left = 137;
    const string soundBank = "Falcon Factory";
    ViewModel.WindowLocationService.Left = left;
    ViewModel.BatchViewModel.Scope.SoundBank = soundBank;
    ViewModel.SimulatedNewColourSchemeId = colourSchemeId;
    await ViewModel.SelectColourSchemeCommand.ExecuteAsync(null);
    Assert.That(ViewModel.ColourSchemeId, Is.EqualTo(colourSchemeId));
    await ViewModel.QueryCloseWindow();
    Assert.That(ViewModel.Settings.WindowLocation.Left, Is.EqualTo(left));
    Assert.That(ViewModel.Settings.Batch.Scope.SoundBank, Is.EqualTo(soundBank));
    Assert.That(ViewModel.Settings.ColourSchemeId, Is.EqualTo(colourSchemeId));
  }

  [Test]
  public async Task ColourSchemeInvalid() {
    Settings = ReadMockSettings("InvalidColourSchemeSettings.xml");
    await ViewModel.Open();
    Assert.That(ViewModel.ColourSchemeId, Is.EqualTo(ColourSchemeId.Lavender));
  }

  [Test]
  public async Task ColourSchemeNotFound() {
    Settings = ReadMockSettings("LocationsSettings.xml");
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
  public async Task FirstTimeLoaded() {
    // Settings folder location not known.
    MockSettingsFolderLocationReader.SimulatedFileExists = false;
    // Show Batch Script tab initially.
    ViewModel.SelectedTab = BatchScriptTab;
    // As the location of the settings file is not known,
    // the window location, set by the view, will have to be saved when the application
    // closes.
    // And an error message box should be shown, then the Locations page.
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
    // All window locations must be set in order for them to be persisted.
    ViewModel.WindowLocationService.Left = left;
    ViewModel.WindowLocationService.Top = top;
    ViewModel.WindowLocationService.Width = width;
    ViewModel.WindowLocationService.Height = height;
    ViewModel.WindowLocationService.WindowState = windowState;
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Folder locations must be specified in the settings."));
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    // Simulate selecting a Settings folder.
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Settings";
    var command =
      (AsyncRelayCommand)ViewModel.LocationsViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    bool canClose = await ViewModel.QueryCloseWindow();
    Assert.That(canClose);
    Assert.That(ViewModel.Settings.WindowLocation, Is.Not.Null);
    Assert.That(ViewModel.Settings.WindowLocation.Left, Is.EqualTo(left));
    Assert.That(ViewModel.Settings.WindowLocation.Top, Is.EqualTo(top));
    Assert.That(ViewModel.Settings.WindowLocation.Width, Is.EqualTo(width));
    Assert.That(ViewModel.Settings.WindowLocation.Height, Is.EqualTo(height));
    Assert.That(ViewModel.Settings.WindowLocation.WindowState, Is.EqualTo(windowState));
  }

  [Test]
  public void GoToLocationsPage() {
    // Show Batch Script tab initially.
    ViewModel.SelectedTab = BatchScriptTab;
    // The GUI Script Processor page's initial validation will fail, resulting in the
    // page immediately being replaced with the Locations page.
    ViewModel.SelectedTab =
      GuiScriptProcessorTab; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    Assert.That(ViewModel.SelectedTab, Is.SameAs(LocationsTab));
  }

  [Test]
  public async Task Main() {
    // Show Batch Script tab initially.
    ViewModel.SelectedTab = BatchScriptTab;
    Assert.That(BatchScriptTab.Header,
      Is.EqualTo(ViewModel.BatchViewModel.TabTitle));
    Assert.That(LocationsTab.Header,
      Is.EqualTo(ViewModel.LocationsViewModel.TabTitle));
    Assert.That(GuiScriptProcessorTab.Header,
      Is.EqualTo(ViewModel.GuiScriptProcessorViewModel.TabTitle));
    Assert.That(MidiForMacrosTab.Header,
      Is.EqualTo(ViewModel.MidiForMacrosViewModel.TabTitle));
    Assert.That(MockCursorService.ShowDefaultCursorCount, Is.EqualTo(2));
    Assert.That(MockCursorService.ShowWaitCursorCount, Is.EqualTo(1));
    Settings = ReadMockSettings("BatchSettings.xml");
    TestGuiScriptProcessorViewModel.ConfigureMockFileSystemService(Settings);
    var selectedPageViewModel = TestGuiScriptProcessorViewModel;
    MockCursorService.ShowDefaultCursorCount = 0;
    MockCursorService.ShowWaitCursorCount = 0;
    ViewModel.SelectedTab =
      GuiScriptProcessorTab; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.Header, Is.EqualTo(selectedPageViewModel.TabTitle));
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(selectedPageViewModel.PageTitle));
    Assert.That(MockCursorService.ShowDefaultCursorCount, Is.EqualTo(1));
    Assert.That(MockCursorService.ShowWaitCursorCount, Is.EqualTo(1));
    await ViewModel.QueryCloseWindow();
    Assert.That(selectedPageViewModel.ClosedCount, Is.EqualTo(1));
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
}