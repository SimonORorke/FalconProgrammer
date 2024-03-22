using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class GuiScriptProcessorViewModelTestsOld  : ViewModelTestsBase{
  [SetUp]
  public override void Setup() {
    base.Setup();
    MockView.ExecuteDispatchAction = true;
    // MockView.ExecuteInvokeAsyncAction = true;
    ViewModel = new GuiScriptProcessorViewModel {
      View = MockView,
      ServiceHelper = ServiceHelper
    };
  }

  private GuiScriptProcessorViewModel ViewModel { get; set; } = null!;

  [Test]
  public void Test1() {
    ViewModel.OnAppearing();
    Assert.That(MockView.DispatchCount, Is.EqualTo(1));
  }
}