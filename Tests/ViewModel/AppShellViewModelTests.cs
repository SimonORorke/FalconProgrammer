using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class AppShellViewModelTests : ViewModelTestsBase {
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new AppShellViewModel(MockDialogWrapper, MockDispatcherService) {
      View = MockView,
      ServiceHelper = ServiceHelper
    };
  }

  private AppShellViewModel ViewModel { get; set; } = null!;

  [Test]
  public void Main() {
    ServiceHelper.CurrentPageTitle = "Locations";
    ViewModel.OnNavigated();
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(ServiceHelper.CurrentPageTitle));
  }
}