using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class TestGuiScriptProcessorViewModel : GuiScriptProcessorViewModel {
  public TestGuiScriptProcessorViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [PublicAPI] internal int ClosedCount { get; set; }

  private MockFileSystemService MockFileSystemService =>
    (MockFileSystemService)FileSystemService;
  
  internal void ConfigureMockFileSystemService(Settings settings) {
    TestHelper.AddSoundBankSubfolders(
      MockFileSystemService.Folder, settings.ProgramsFolder.Path);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    bool result = await base.QueryClose(isClosingWindow);
    if (result) {
      ClosedCount++;
    }
    return result;
  }
}