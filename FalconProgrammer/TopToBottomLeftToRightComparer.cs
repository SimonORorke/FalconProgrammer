using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer; 

/// <summary>
///   Orders the ConstantModulations top to bottom, left to right.
/// </summary>
public class TopToBottomLeftToRightComparer : Comparer<ConstantModulation> {
  public override int Compare(ConstantModulation? a, ConstantModulation? b) {
    if (a!.Properties.Y < b!.Properties.Y) {
      return -1;
    }
    if (a.Properties.Y > b.Properties.Y) {
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