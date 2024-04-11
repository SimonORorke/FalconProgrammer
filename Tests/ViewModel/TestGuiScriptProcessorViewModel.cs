using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.ViewModel;

public class TestGuiScriptProcessorViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : GuiScriptProcessorViewModel(dialogService, dispatcherService) {
  [PublicAPI] internal int ClosedCount { get; set; }
  internal bool SkipInitialisation { get; set; }

  [ExcludeFromCodeCoverage]
  internal override void Open() {
    if (!SkipInitialisation) {
      base.Open();
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