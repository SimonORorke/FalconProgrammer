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
  public override void Open() {
    if (!SkipInitialisation) {
      base.Open();
    }
  }

  public override bool QueryClose() {
    bool result = base.QueryClose();
    if (result) {
      ClosedCount++;
    }
    return result;
  }
}