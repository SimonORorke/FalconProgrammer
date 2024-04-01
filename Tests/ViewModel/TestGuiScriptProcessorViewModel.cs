using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class TestGuiScriptProcessorViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : GuiScriptProcessorViewModel(dialogWrapper, dispatcherService) {
  internal int OnDisappearingCount { get; set; }
  internal bool SkipInitialisation { get; set; }

  [ExcludeFromCodeCoverage]
  protected override void Initialise() {
    if (!SkipInitialisation) {
      base.Initialise();
    }
  }
  
  public override void OnDisappearing() {
    base.OnDisappearing();
    OnDisappearingCount++;
  }
}