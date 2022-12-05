using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlDeserialised;

public class SignalConnection {
  public SignalConnection() {
    Ratio = "1";
    Source = string.Empty;
    Destination = "Value";
  }
  
  [XmlAttribute] public string Ratio { get; set; }
  [XmlAttribute] public string Source { get; set; }
  [XmlAttribute] public string Destination { get; set; }

  [PublicAPI] public int CcNo {
    get =>
      Convert.ToInt32(
        Source.Replace("@MIDI CC ", string.Empty));
    set => Source = $"@MIDI CC {value}";
  }

  [PublicAPI] public int? MacroNo {
    get =>
      Destination.StartsWith("Macro")
        ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
        : null;
    set => Destination = $"Macro{value}";
  }
}