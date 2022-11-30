using System.Xml.Serialization;

namespace FalconProgrammer.XmlModels;

public class SignalConnection {
  [XmlAttribute] public string Source { get; set; } = null!;
  [XmlAttribute] public string Destination { get; set; } = null!;

  public int CcNo => Convert.ToInt32(
    Source.Replace("@MIDI CC ", string.Empty));

  public int? MacroNo =>
    Destination.StartsWith("Macro")
      ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
      : null;
}