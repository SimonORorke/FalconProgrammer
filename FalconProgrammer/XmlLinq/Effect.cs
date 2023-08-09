using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public class Effect : ConnectionsParent {
  private bool? _isDelay;
  private bool? _isReverb;

  public Effect(XElement connectionsParentElement, ProgramXml programXml) : base(
    connectionsParentElement, programXml) {
    EffectType = connectionsParentElement.Name.ToString();
  }

  public string EffectType { get; }
  public bool IsDelay => _isDelay ??= GetIsDelay();
  public bool IsReverb => _isReverb ??= GetIsReverb();

  private bool GetIsDelay() {
    // "Buzz" is Analog Tape Delay!
    return EffectType is "Buzz" or "DualDelay" or "FatDelay" or "FxDelay"
      or "PingPongDelay" or "SimpleDelay" or "StereoDelay" or "TapeEcho";
  }

  private bool GetIsReverb() {
    return EffectType is "DelayedReverb" or "FilteredReverb" or "PlainReverb"
      or "SampledReverb" or "SharpVerb" or "TapeEcho";
  }
}