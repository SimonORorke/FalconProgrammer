using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class TestMainWindowViewModel(
  IDialogWrapper dialogWrapper,
  IDispatcherService dispatcherService)
  : MainWindowViewModel(dialogWrapper, dispatcherService) {

  internal override GuiScriptProcessorViewModel GuiScriptProcessorViewModel { get; } =
  new TestGuiScriptProcessorViewModel(dialogWrapper, dispatcherService);

  internal TestGuiScriptProcessorViewModel TestGuiScriptProcessorViewModel =>
    (TestGuiScriptProcessorViewModel)GuiScriptProcessorViewModel;
}