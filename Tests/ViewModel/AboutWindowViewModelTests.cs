using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class AboutWindowViewModelTests {
  [Test]
  public void Main() {
    var mockApplicationInfo = new MockApplicationInfo();
    var viewModel = new AboutWindowViewModel {
      ApplicationInfo = mockApplicationInfo
    };
    Assert.That(viewModel.Copyright, Is.EqualTo(mockApplicationInfo.Copyright));
    Assert.That(viewModel.Product, Is.EqualTo(mockApplicationInfo.Product));
    Assert.That(viewModel.Title, Is.EqualTo("About Falcon Programmer"));
    Assert.That(viewModel.Version, Is.EqualTo(mockApplicationInfo.Version));
    bool hasClosed = false;
    viewModel.MustClose += (_, _) => hasClosed = true;
    viewModel.OkCommand.Execute(null);
    Assert.That(hasClosed);
  }
}