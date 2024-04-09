using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoViewModelTests {
  [Test]
  public void Validate() {
    const string caption = "Test";
    const int invalidCcNo = 128;
    var viewModel = new CcNoViewModel(caption);
    Assert.That(viewModel.Caption, Is.EqualTo(caption));
    viewModel.CcNo = invalidCcNo;
    var errors = viewModel.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(1));
    var memberNames = errors[0].MemberNames.ToList();
    Assert.That(memberNames, Has.Count.EqualTo(1));
    Assert.That(memberNames[0], Is.EqualTo(nameof(viewModel.CcNo)));
    Assert.That(viewModel.CcNo, Is.EqualTo(invalidCcNo));
  }
}