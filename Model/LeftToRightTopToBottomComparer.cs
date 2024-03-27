using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Model;

/// <summary>
///   Orders the macros left to right, top to bottom.
/// </summary>
internal class LeftToRightTopToBottomComparer : Comparer<Macro> {
  public override int Compare(Macro? a, Macro? b) {
    if (a!.X < b!.X) {
      return -1;
    }
    if (a.X > b.X) {
      return 1;
    }
    if (a.Y < b.Y) {
      return -1;
    }
    if (a.Y > b.Y) {
      return 1;
    }
    throw new ApplicationException(
      "Duplicate ConstantModulation.Properties location: X = " +
      $"{a.X}; Y = {a.Y}. " +
      $"a = {a}. b = {b}.");
  }
}