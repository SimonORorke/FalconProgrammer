using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer; 

/// <summary>
///   Sorts the ConstantModulations top to bottom, left to right.
/// </summary>
public class ConstantModulationLocationComparer : Comparer<ConstantModulation> {
  public override int Compare(ConstantModulation? a, ConstantModulation? b) {
    if (a.Properties.Y < b.Properties.Y) {
      return -1;
    }
    if (a.Properties.Y > b.Properties.Y) {
      return 1;
    }
    if (a.Properties.X < b.Properties.X) {
      return -1;
    }
    return a.Properties.X > b.Properties.X ? 1 : 0;
  }
}