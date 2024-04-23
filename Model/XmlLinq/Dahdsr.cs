using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   A Delay Attack Hold Decay Sustain Release modulator.
/// </summary>
public class Dahdsr : Effect {
  /// <summary>
  ///   A Delay Attack Hold Decay Sustain Release modulator.
  /// </summary>
  public Dahdsr(XElement element, ProgramXml programXml) : base(element, programXml) { }

  public float AttackTime {
    get => Convert.ToSingle(GetAttributeValue(nameof(AttackTime)));
    set => SetAttribute(nameof(AttackTime), value);
  }

  public float ReleaseTime {
    get => Convert.ToSingle(GetAttributeValue(nameof(ReleaseTime)));
    set => SetAttribute(nameof(ReleaseTime), value);
  }
}