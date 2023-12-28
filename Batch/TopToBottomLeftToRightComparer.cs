using FalconProgrammer.Batch.XmlLinq;

namespace FalconProgrammer.Batch; 

/// <summary>
///   Orders the macros top to bottom, left to right.
/// </summary>
public class TopToBottomLeftToRightComparer : Comparer<Macro> {
  public override int Compare(Macro? a, Macro? b) {
    // We need to judge macros whose tops are close together as being in the same row.
    // The vertical clearance is 95, so this should be safe.
    // Example: "Ether Fields\Wavetable\Vocal Lead Synth".
    const int verticalFudge = 30;
    if (a!.Y < b!.Y - verticalFudge) {
      return -1;
    }
    if (a.Y > b.Y + verticalFudge) {
      return 1;
    }
    if (a.X < b.X) {
      return -1;
    }
    if (a.X > b.X) {
      return 1;
    }
    throw new ApplicationException(
      "Duplicate ConstantModulation.Properties location: X = " + 
      $"{a.X}; Y = {a.Y}. " + 
      $"a = {a}. b = {b}.");
  }
}