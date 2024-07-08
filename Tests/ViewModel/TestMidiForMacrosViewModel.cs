using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class TestMidiForMacrosViewModel : MidiForMacrosViewModel {
  public TestMidiForMacrosViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  private MockFileSystemService MockFileSystemService =>
    (MockFileSystemService)FileSystemService;

  internal void ConfigureMockFileSystemService(Settings settings) {
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, settings.ProgramsFolder.Path);
  }
}