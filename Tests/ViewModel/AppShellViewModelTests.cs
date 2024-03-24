using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class AppShellViewModelTests : ViewModelTestsBase {

  private AppShellViewModel ViewModel { get; set; } = null!;
  
  [SetUp]
  public override void Setup() {
    base.Setup();
    ViewModel = new AppShellViewModel {
      View = MockView,
      ServiceHelper = ServiceHelper
    };
  }
  
  [Test]
  public void Main() {
    ServiceHelper.CurrentPageTitle = "Locations";
    ViewModel.OnNavigated();
    Assert.That(ViewModel.CurrentPageTitle, Is.EqualTo(ServiceHelper.CurrentPageTitle));
  }
}