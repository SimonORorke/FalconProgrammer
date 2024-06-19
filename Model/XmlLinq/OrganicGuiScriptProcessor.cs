using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   GUI ScriptProcessor for the "Organic Keys" sound bank.
///   Base GUI ScriptProcessor for the "Organic Pads" sound bank.
/// </summary>
internal class OrganicGuiScriptProcessor : ScriptProcessor {
  /// <summary>
  ///   Use the <see cref="ScriptProcessor.Create" /> static method for public
  ///   instantiation of the correct type of <see cref="ScriptProcessor" />.
  /// </summary>
  public OrganicGuiScriptProcessor(XElement scriptProcessorElement,
    ProgramXml programXml, MidiForMacros midi)
    : base(scriptProcessorElement, programXml, midi) { }

  public float DelaySend {
    get => Convert.ToSingle(GetAttributeValue("delaySend"));
    set => SetAttribute("delaySend", value);
  }

  public float ReverbSend {
    get => Convert.ToSingle(GetAttributeValue("reverbSend"));
    set => SetAttribute("reverbSend", value);
  }
}