using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoRangeItemTests {
  [SetUp]
  public void Setup() {
    Item = TestHelper.CreateCcNoRangeAdditionItem(0, 127);
  }

  private CcNoRangeItem Item { get; set; } = null!;

  [Test]
  public void ValidateStart() {
    Item.End = 1;
    Item.Start = 127;
    var errors = Item.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(1));
    var memberNames = errors[0].MemberNames.ToList();
    Assert.That(memberNames, Has.Count.EqualTo(1));
    Assert.That(memberNames[0], Is.EqualTo(nameof(Item.Start)));
  }

  [Test]
  public void ValidateEnd() {
    Item.Start = 127;
    Item.End = 1;
    var errors = Item.GetErrors().ToList();
    Assert.That(errors, Has.Count.EqualTo(1));
    var memberNames = errors[0].MemberNames.ToList();
    Assert.That(memberNames, Has.Count.EqualTo(1));
    Assert.That(memberNames[0], Is.EqualTo(nameof(Item.End)));
  }
}