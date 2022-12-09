using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised;

public class Properties {
  
  /// <summary>
  ///   An optional attribute that, if present, seems always to have the value "0".
  ///   A deserialisation exception would be thrown when the attribute is absent, if we
  ///   were to give it the type int? instead of string?. 
  /// </summary>
  [XmlAttribute("showValue")] public string? ShowValue { get; set; }
  
  [XmlAttribute("x")] public int X { get; set; }
  [XmlAttribute("y")] public int Y { get; set; }

  /// <summary>
  ///   Gets whether the coordinates specified in the Properties are actually to be used
  ///   to determine the location of the macro on the Info page. False if the optional
  ///   showValue attribute is included in the Properties XML element and set to "0"
  ///   (showValue="0"), in which case the SignalConnection mapping the MIDI CC number to
  ///   the macro must be added, via <see cref="ScriptConfig"/>, to the ScriptProcessor
  ///   for the script that defines the Info page layout. 
  /// </summary>
  private bool DeterminesMacroLocationOnInfoPage => ShowValue != "0";

  public void Validate() {
    if (!DeterminesMacroLocationOnInfoPage) {
      throw new ApplicationException(
        "ConstantModulation.Properties include the optional attribute " + 
        "showValue=\"0\", indicating that the coordinates specified in the Properties " +
        "will not actually to be used to determine the locations of macros on the " + 
        "Info page. Instead, the SignalConnections mapping MIDI CC numbers to " +
        "macros must be added, via ScriptConfig, to the ScriptProcessor for the " + 
        "script that defines the Info page layout.");
    }
  }
}