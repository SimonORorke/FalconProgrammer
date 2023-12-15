using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public class Effect(XElement element, ProgramXml programXml)
  : ConnectionsParent(element, programXml) {
  private bool? _isDelay;
  private bool? _isReverb;
  public string EffectType { get; } = element.Name.ToString();
  public bool IsDelay => _isDelay ??= GetIsDelay();
  public bool IsReverb => _isReverb ??= GetIsReverb();

  private bool GetIsDelay() {
    // "Buzz" is Analog Tape Delay!
    return EffectType is "Buzz" or "DualDelay" or "DualDelayX" or "FatDelay" or "FxDelay"
      or "PingPongDelay" or "SimpleDelay" or "StereoDelay" or "TapeEcho";
  }

  private bool GetIsReverb() {
    return EffectType is "DelayedReverb" or "FilteredReverb" or "PlainReverb"
      or "SampledReverb" or "SharpVerb" or "TapeEcho";
  }
}