using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq; 

/// <summary>
///   A Delay Attack Hold Decay Sustain Release modulator.
/// </summary>
public class Dahdsr : Effect {
  public Dahdsr(XElement element, ProgramXml programXml) : base(element, programXml) { }
  
  public float AttackTime {
    get => Convert.ToSingle(GetAttributeValue(nameof(AttackTime)));
    set => SetAttribute(nameof(AttackTime), value);
  }
  
  public string DisplayName {
    get => GetAttributeValue(nameof(DisplayName));
    set => SetAttribute(nameof(DisplayName), value);
  }

  public float ReleaseTime {
    get => Convert.ToSingle(GetAttributeValue(nameof(ReleaseTime)));
    set => SetAttribute(nameof(ReleaseTime), value);
  }
}