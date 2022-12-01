using System.Xml.Serialization;

namespace FalconProgrammer.XmlModels;

public class SignalConnection {
  [XmlAttribute] public string Ratio { get; set; } = null!;
  [XmlAttribute] public string Source { get; set; } = null!;
  [XmlAttribute] public string Destination { get; set; } = null!;

  public int CcNo {
    get =>
      Convert.ToInt32(
        Source.Replace("@MIDI CC ", string.Empty));
    set => Source = $"@MIDI CC {value}";
  }

  public int? MacroNo {
    get =>
      Destination.StartsWith("Macro")
        ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
        : null;
    set => Destination = $"Macro{value}";
  }
}