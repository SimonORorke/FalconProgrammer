using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised;

public class SignalConnection {
  public SignalConnection() {
    Ratio = 1;
    Source = string.Empty;
    Destination = "Value";
  }

  [XmlAttribute] public float Ratio { get; set; }
  [XmlAttribute] public string Source { get; set; }
  [XmlAttribute] public string Destination { get; set; }
  [XmlAttribute] public int ConnectionMode { get; set; }

  public int? CcNo {
    get =>
      Source.StartsWith("@MIDI CC ")
        ? Convert.ToInt32(Source.Replace("@MIDI CC ", string.Empty))
        : null;
    set => Source = $"@MIDI CC {value}";
  }

  public int Index { get; set; }

  /// <summary>
  ///   Gets whether the MIDI CC mapping is to control a macro on the Info page.
  ///   So far, the only CC mappings that are not for Info page controls are for the
  ///   modulation wheel (MIDI CC 1).
  /// </summary>
  public bool IsForInfoPageMacro => ConnectionMode == 1;

  public int? MacroNo {
    get =>
      Destination.StartsWith("Macro")
        ? Convert.ToInt32(Destination.Replace("Macro", string.Empty))
        : null;
    set => Destination = $"Macro{value}";
  }
}