using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoRangeViewModelTests {
  [SetUp]
  public void Setup() {
    ViewModel = TestHelper.CreateCcNoRangeAdditionItem(0, 127);
  }

  private CcNoRangeViewModel ViewModel { get; set; } = null!;

  [Test]
  public void ValidateStart() {
    ViewModel.End = 1;
    ViewModel.Start = 127;
    var errors = ViewModel.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(1));
    var memberNames = errors[0].MemberNames.ToList();
    Assert.That(memberNames, Has.Count.EqualTo(1));
    Assert.That(memberNames[0], Is.EqualTo(nameof(ViewModel.Start)));
  }

  [Test]
  public void ValidateEnd() {
    ViewModel.Start = 127;
    ViewModel.End = 1;
    var errors = ViewModel.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(1));
    var memberNames = errors[0].MemberNames.ToList();
    Assert.That(memberNames, Has.Count.EqualTo(1));
    Assert.That(memberNames[0], Is.EqualTo(nameof(ViewModel.End)));
  }
}