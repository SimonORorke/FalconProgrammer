using FalconProgrammer.Model;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class AboutWindowViewModelTests {
  [SetUp]
  public void Setup() {
    MockApplicationInfo = new MockApplicationInfo();
    MockDialogService = new MockDialogService();
    ViewModel = new AboutWindowViewModel(MockDialogService) {
      ApplicationInfo = MockApplicationInfo
    };
  }

  private MockApplicationInfo MockApplicationInfo { get; set; } = null!;
  private MockDialogService MockDialogService { get; set; } = null!;
  private AboutWindowViewModel ViewModel { get; set; } = null!;

  [Test]
  public void ApplicationInfo() {
    Assert.That(ViewModel.Copyright, Is.EqualTo(MockApplicationInfo.Copyright));
    Assert.That(ViewModel.Product, Is.EqualTo(MockApplicationInfo.Product));
    Assert.That(ViewModel.Title, Is.EqualTo("About Falcon Programmer"));
    Assert.That(ViewModel.Version, Does.Contain(MockApplicationInfo.Version));
  }

  [Test]
  public async Task Licence() {
    Global.ApplicationName = new MockApplicationInfo().Product;
    await ViewModel.LicenceCommand.ExecuteAsync(null);
    Assert.That(MockDialogService.ShowMessageWindowCount, Is.EqualTo(1));
    Assert.That(MockDialogService.LastMessageWindowMessage, 
      Does.StartWith(MockApplicationInfo.Product));
    Assert.That(MockDialogService.LastMessageWindowTitle, 
      Is.EqualTo("Falcon Programmer - Licence"));
  }
}