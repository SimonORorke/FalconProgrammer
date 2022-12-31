using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer; 

/// <summary>
///   Orders the macros top to bottom, left to right.
/// </summary>
public class TopToBottomLeftToRightComparer : Comparer<ConstantModulation> {
  public override int Compare(ConstantModulation? a, ConstantModulation? b) {
    // We need to judge macros whose tops are close together as being in the same row.
    // The vertical clearance is 95, so this should be safe.
    // Example: "Ether Fields\Wavetable\Vocal Lead Synth".
    const int verticalFudge = 30;
    if (a!.Properties.Y < b!.Properties.Y - verticalFudge) {
      return -1;
    }
    if (a.Properties.Y > b.Properties.Y + verticalFudge) {
      return 1;
    }
    if (a.Properties.X < b.Properties.X) {
      return -1;
    }
    if (a.Properties.X > b.Properties.X) {
      return 1;
    }
    throw new ApplicationException(
      "Duplicate ConstantModulation.Properties location: X = " + 
      $"{a.Properties.X}; Y = {a.Properties.Y}. " + 
      $"a = {a}. b = {b}.");
  }
}