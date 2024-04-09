using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoRangeViewModelTests {
  [SetUp]
  public void Setup() {
    ViewModel = new CcNoRangeViewModel(
      AppendAdditionItem, OnItemChanged, RemoveItem, false);
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

  [ExcludeFromCodeCoverage]
  private static void AppendAdditionItem() { }

  private static void OnItemChanged() { }

  [ExcludeFromCodeCoverage]
  private static void RemoveItem(ObservableObject itemToRemove) { }
}