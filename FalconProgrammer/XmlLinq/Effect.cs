using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq; 

public class Effect {
  public Effect(XElement signalConnectionElement, ProgramXml programXml) {
    SignalConnection = new SignalConnection(signalConnectionElement);
    EffectElement = signalConnectionElement.Parent!.Parent!;
    ProgramXml = programXml;
    EffectType = EffectElement.Name.ToString();
  }

  public bool Bypass {
    get => ProgramXml.GetAttributeValue(EffectElement,"Bypass") == "1";
    set => ProgramXml.SetAttribute(EffectElement,"Bypass", value ? "1" : "0");
  }
  
  private XElement EffectElement { get; }
  public string EffectType { get; }
  private ProgramXml ProgramXml { get; }
  private SignalConnection SignalConnection { get; }

  public void ChangeModulatedParameterToZero() {
    try {
      ProgramXml.SetAttribute(
        EffectElement, SignalConnection.Destination,
        // If it's a toggle macro, Destination should be "Bypass".  
        SignalConnection.Destination == "Bypass" ? 1 : 0);
      // ReSharper disable once EmptyGeneralCatchClause
    } catch { }
  }
}