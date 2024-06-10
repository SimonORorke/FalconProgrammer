using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

/// <summary>
///   An unrealistic drive letter, different from the one in embedded test settings
///   files, has been chosen in these tests, as they are all expected to access a mock
///   file system. A test will throw an exception if it attempts to write to the real
///   file system, provided it is run on a computer that does not have that drive. -->
/// </summary>
[TestFixture]
public class LocationsViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new LocationsViewModel(MockDialogService, MockDispatcherService) {
      ModelServices = TestModelServices
    };
    // Do not ViewModel.Open this stage: the NoSettingsFolderLocation test
    // requires prior configuration.
  }

  private LocationsViewModel ViewModel { get; set; } = null!;

  [Test]
  public async Task CancelBrowseForSettingsFolder() {
    await ViewModel.Open();
    MockDialogService.Cancel = true;
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Settings";
    MockFileSystemService.File.SimulatedExists = false;
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.Not.EqualTo(MockDialogService.SimulatedPath));
  }

  [Test]
  public async Task LoadSettingsFromAnotherSettingsFile() {
    await ViewModel.Open();
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Settings";
    MockDialogService.SimulatedYesNoAnswer = true;
    string newSettingsPath =
      Path.Combine(MockDialogService.SimulatedPath, "Settings.xml");
    Assert.That(ViewModel.Settings.SettingsPath, Is.Not.EqualTo(newSettingsPath));
    MockSettingsFolderLocationReader.EmbeddedFileName =
      "SettingsFolderLocationK.xml";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(MockDialogService.AskYesNoQuestionCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastYesNoAnswer, Is.True);
    Assert.That(ViewModel.Settings.SettingsPath, Is.EqualTo(newSettingsPath));
  }

  [Test]
  public async Task Main() {
    await ViewModel.Open();
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Settings";
    var command = (AsyncRelayCommand)ViewModel.BrowseForSettingsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.SettingsFolderPath,
      Is.EqualTo(MockDialogService.SimulatedPath));
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Programs";
    MockFileSystemService.File.SimulatedExists = false;
    command = (AsyncRelayCommand)ViewModel.BrowseForProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.ProgramsFolderPath,
      Is.EqualTo(MockDialogService.SimulatedPath));
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Original Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForOriginalProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.OriginalProgramsFolderPath,
      Is.EqualTo(MockDialogService.SimulatedPath));
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Template Programs";
    command = (AsyncRelayCommand)ViewModel.BrowseForTemplateProgramsFolderCommand;
    await command.ExecuteAsync(null);
    Assert.That(ViewModel.TemplateProgramsFolderPath,
      Is.EqualTo(MockDialogService.SimulatedPath));
    bool canClose = await ViewModel.QueryClose();
    Assert.That(canClose);
    var mockSerialiser = (MockSerialiser)ViewModel.Settings.Serialiser;
    Assert.That(mockSerialiser.LastOutputPath,
      Is.EqualTo(@"K:\NewLeaf\Settings\Settings.xml"));
    Assert.That(mockSerialiser.LastType, Is.EqualTo(typeof(Settings)));
    var settings = (Settings)mockSerialiser.LastObjectSerialised;
    Assert.That(settings.SettingsPath,
      Is.EqualTo(Path.Combine(ViewModel.SettingsFolderPath, "Settings.xml")));
    Assert.That(settings.ProgramsFolder.Path, Is.EqualTo(ViewModel.ProgramsFolderPath));
    Assert.That(settings.OriginalProgramsFolder.Path,
      Is.EqualTo(ViewModel.OriginalProgramsFolderPath));
    Assert.That(settings.TemplateProgramsFolder.Path,
      Is.EqualTo(ViewModel.TemplateProgramsFolderPath));
  }

  [Test]
  public async Task NoSettingsFolderLocation() {
    MockSettingsFolderLocationReader.SimulatedFileExists = false;
    await ViewModel.Open();
    Assert.That(ViewModel.SettingsFolderPath, Is.Empty);
  }

  [Test]
  public async Task SettingsFolderDoesNotExist() {
    await ViewModel.Open();
    ViewModel.SettingsFolderPath = @"K:\NewLeaf\Settings";
    MockFileSystemService.File.SimulatedExists = false;
    MockFileSystemService.Folder.SimulatedExists = false;
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Programs";
    // Make a property change to require saving settings.
    ViewModel.ProgramsFolderPath += "X";
    Assert.That(await ViewModel.QueryClose(), Is.False);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Settings cannot be saved: cannot find settings folder " +
      $"'{ViewModel.SettingsFolderPath}'."));
  }

  [Test]
  public async Task SettingsFolderNotSpecified() {
    await ViewModel.Open(); // All folder fields are empty.
    MockFileSystemService.File.SimulatedExists = false;
    MockDialogService.SimulatedPath = @"K:\NewLeaf\Programs";
    ViewModel.SettingsFolderPath = string.Empty;
    Assert.That(await ViewModel.QueryClose(), Is.False);
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Is.EqualTo(
      "Settings cannot be saved: a settings folder has not been specified."));
  }

  [Test]
  public async Task SettingsXmlError() {
    MockSettingsReaderEmbedded.EmbeddedFileName = "InvalidXmlSettings.xml";
    await ViewModel.Open();
    Assert.That(MockDialogService.ShowErrorMessageBoxCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastErrorMessage, Does.StartWith(
      "Invalid XML was found in embedded file '"));
  }

  [Test]
  public async Task ValidationErrorsOnQueryClose() {
    var settings = ReadMockSettings("LocationsSettings.xml");
    MockFileSystemService.Folder.ExistingPaths.Add(settings.SettingsPath);
    await ViewModel.Open();
    ViewModel.ProgramsFolderPath = @"K:\Test\Programs";
    ViewModel.OriginalProgramsFolderPath = string.Empty;
    ViewModel.TemplateProgramsFolderPath = @"K:\Test\Template Programs";
    Assert.That(ViewModel.HasErrors);
    var errors = ViewModel.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(3));
    // The errors are alphabetical by property name.
    Assert.That(errors[0].ErrorMessage, Is.EqualTo(
      "The OriginalProgramsFolderPath field is required."));
    Assert.That(errors[1].MemberNames.ToList()[0], Is.EqualTo("ProgramsFolderPath"));
    Assert.That(errors[1].ErrorMessage, Is.EqualTo("Cannot find folder."));
    Assert.That(errors[2].ErrorMessage, Is.EqualTo("Cannot find folder."));
    bool canClose = await ViewModel.QueryClose();
    Assert.That(!canClose);
  }
}