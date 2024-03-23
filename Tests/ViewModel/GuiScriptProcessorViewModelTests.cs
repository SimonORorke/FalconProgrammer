using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class GuiScriptProcessorViewModelTests : ViewModelTestsBase {
  private GuiScriptProcessorViewModel ViewModel { get; set; } = null!;

  [SetUp]
  public override void Setup() {
    base.Setup();
    MockView.ExecuteDispatchAction = true;
    ViewModel = new GuiScriptProcessorViewModel {
      View = MockView,
      ServiceHelper = ServiceHelper,
      SettingsReader = TestSettingsReader
    };
  }

  [Test]
  public void NoProgramsFolder() {
    ViewModel.OnAppearing();
    Assert.That(MockAlertService.ShowAlertCount, Is.EqualTo(1));
    Assert.That(MockAlertService.LastMessage, Is.EqualTo(
      "Script processors cannot be updated: a programs folder has not been specified."));
    Assert.That(MockView.DispatchCount, Is.EqualTo(1));
  }
}