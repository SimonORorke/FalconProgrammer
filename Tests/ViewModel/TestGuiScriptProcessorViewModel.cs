using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class TestGuiScriptProcessorViewModel : GuiScriptProcessorViewModel {
  public TestGuiScriptProcessorViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [PublicAPI] internal int ClosedCount { get; set; }
  internal bool SkipInitialisation { get; set; }

  [ExcludeFromCodeCoverage]
  internal override async Task Open() {
    if (!SkipInitialisation) {
      await base.Open();
    }
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    bool result = await base.QueryClose(isClosingWindow);
    if (result) {
      ClosedCount++;
    }
    return result;
  }
}