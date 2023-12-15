using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq; 

/// <summary>
///   A Delay Attack Hold Decay Sustain Release modulator.
/// </summary>
public class Dahdsr(XElement element, ProgramXml programXml)
  : Effect(element, programXml) {
  public float AttackTime {
    get => Convert.ToSingle(GetAttributeValue(nameof(AttackTime)));
    set => SetAttribute(nameof(AttackTime), value);
  }

  public float ReleaseTime {
    get => Convert.ToSingle(GetAttributeValue(nameof(ReleaseTime)));
    set => SetAttribute(nameof(ReleaseTime), value);
  }
}