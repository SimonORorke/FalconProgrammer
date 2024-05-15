using FalconProgrammer.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class LicenceViewModelTests {
  [Test]
  public void Main() {
    var viewModel = new LicenceViewModel();
    Assert.That(viewModel.Licence, Does.StartWith(Global.ApplicationName));
  }
}