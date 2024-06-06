using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class ProgramItemTests : ViewModelTestsBase {
  [Test]
  public void CategoryFolderNotFound() {
    MockFileSystemService.Folder.ThrowIfNoSimulatedSubfolders = true;
    var settings = ReadMockSettings("BatchSettings.xml");
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, settings.ProgramsFolder.Path);
    const string soundBank = "Falcon Factory";
    const string category = "Bass-Sub";
    var item = new ProgramItem(
      settings, MockFileSystemService, false, false) {
      SoundBank = soundBank
    };
    Assert.DoesNotThrow(()=> item.Category = category);
  }
  
  [Test]
  public void SoundBankFolderNotFound() {
    var settings = ReadMockSettings("BatchSettings.xml");
    MockFileSystemService.Folder.ExistingPaths.Add(settings.ProgramsFolder.Path);
    const string soundBank = "Falcon Factory";
    var item = new ProgramItem(
      settings, MockFileSystemService, false, false) {
    };
    Assert.DoesNotThrow(()=> item.SoundBank = soundBank);
    Assert.That(item.Categories, Is.Empty);
  }
}