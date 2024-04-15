using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class MainWindowViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    TestGuiScriptProcessorViewModel = new TestGuiScriptProcessorViewModel(
      MockDialogService, MockDispatcherService);
    ViewModel = new MainWindowViewModel(
      MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices,
      GuiScriptProcessorViewModel = TestGuiScriptProcessorViewModel
    };
    // Do not show the Locations tab initially at this stage: the SettingsXmlError test
    // requires prior configuration.
  }

  private TestGuiScriptProcessorViewModel TestGuiScriptProcessorViewModel { get; set; } =
    null!;

  private MainWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void ApplicationTitle() {
    ViewModel.ApplicationTitle = "A made-up name";
    Assert.That(Global.ApplicationTitle, Is.EqualTo(ViewModel.ApplicationTitle));
  }

  [Test]
  public void DisallowChangePage() {
    // Show Locations tab initially.
    ViewModel.SelectedTab = ViewModel.Tabs[0];
    // Unspecified settings folder is an error condition that should force the user to
    // stay on the Locations page.
    ViewModel.LocationsViewModel.SettingsFolderPath = string.Empty;
    // Try going to another page.
    ViewModel.SelectedTab = ViewModel.Tabs[1]; // Test GUI Script Processor view model
    // An error message box should have been shown and the Locations page should still be
    // shown.
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    Assert.That(ViewModel.SelectedTab, Is.SameAs(ViewModel.Tabs[0]));
  }

  [Test]
  public async Task Main() {
    // Show Locations tab initially.
    ViewModel.SelectedTab = ViewModel.Tabs[0];
    Assert.That(ViewModel.Tabs[0].Header,
      Is.EqualTo(ViewModel.LocationsViewModel.TabTitle));
    Assert.That(ViewModel.Tabs[1].Header,
      Is.EqualTo(ViewModel.GuiScriptProcessorViewModel.TabTitle));
    Assert.That(ViewModel.Tabs[2].Header,
      Is.EqualTo(ViewModel.MidiForMacrosViewModel.TabTitle));
    Assert.That(ViewModel.Tabs[3].Header,
      Is.EqualTo(ViewModel.BatchScriptViewModel.TabTitle));
    // Skip the GUI Script Processor page's initial validation.
    // Otherwise the validation might fail, resulting in the page immediately being
    // replaced with the Locations page.
    TestGuiScriptProcessorViewModel.SkipInitialisation = true;
    var selectedPageViewModel = TestGuiScriptProcessorViewModel;
    ViewModel.SelectedTab = ViewModel.Tabs[1]; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.Header, Is.EqualTo(selectedPageViewModel.TabTitle));
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(selectedPageViewModel.PageTitle));
    await ViewModel.QueryCloseWindowAsync();
    Assert.That(selectedPageViewModel.ClosedCount, Is.EqualTo(1));
  }

  [Test]
  public void GoToLocationsPage() {
    // Show Locations tab initially.
    ViewModel.SelectedTab = ViewModel.Tabs[0];
    // The GUI Script Processor page's initial validation will fail, resulting in the
    // page immediately being replaced with the Locations page.
    ViewModel.SelectedTab = ViewModel.Tabs[1]; // Test GUI Script Processor view model 
    Assert.That(ViewModel.SelectedTab.ViewModel, Is.SameAs(ViewModel.LocationsViewModel));
    Assert.That(ViewModel.SelectedTab, Is.SameAs(ViewModel.Tabs[0]));
  }

  [Test]
  public void SettingsXmlError() {
    MockSettingsFolderLocationReader.TestDeserialiser.EmbeddedResourceFileName =
    // TestSettingsReaderEmbedded.EmbeddedSettingsFolderLocationFileName =
      "InvalidXmlSettings.xml";
    ViewModel.SelectedTab = ViewModel.Tabs[0]; // Locations
    Assert.That(ViewModel.LocationsViewModel.ModelServices.SettingsReader,
      Is.EqualTo(TestSettingsReaderEmbedded));
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
  }

  [Test]
  public async Task UserConfirmsCloseWindowWhenError() {
    ViewModel.SelectedTab = ViewModel.Tabs[0]; // Locations
    // Error condition: settings folder not found.
    // So the user should be prompted to confirm closing the window.
    MockFileSystemService.Folder.ExpectedExists = false;
    // User will confirm that the window should be closed, even though there is a error.
    MockDialogService.ExpectedYesNoAnswer = true;
    // Make a property change to require saving settings.
    ViewModel.LocationsViewModel.ProgramsFolderPath += "X";
    bool canClose = await ViewModel.QueryCloseWindowAsync();
    // Question message box shown.
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.EqualTo(1));
    Assert.That(canClose, Is.True);
  }

  [Test]
  public async Task UserCancelsCloseWindowWhenError() {
    ViewModel.SelectedTab = ViewModel.Tabs[0]; // Locations
    // Error condition: settings folder not found.
    // So the user should be prompted to confirm closing the window.
    MockFileSystemService.Folder.ExpectedExists = false;
    // User will decline to close the window, so that the error can be fixed.
    MockDialogService.ExpectedYesNoAnswer = false;
    // Make a property change to require saving settings.
    ViewModel.LocationsViewModel.ProgramsFolderPath += "X";
    bool canClose = await ViewModel.QueryCloseWindowAsync();
    // Question message box shown.
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.EqualTo(1));
    Assert.That(canClose, Is.False);
  }
}