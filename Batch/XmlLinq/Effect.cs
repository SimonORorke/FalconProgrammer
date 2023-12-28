using System.Xml.Linq;

namespace FalconProgrammer.Batch.XmlLinq;

public class Effect(XElement element, ProgramXml programXml)
  : ConnectionsParent(element, programXml) {
  private bool? _isDelay;
  private bool? _isReverb;
  public string EffectType { get; } = element.Name.ToString();
  public bool IsDelay => _isDelay ??= GetIsDelay();
  public bool IsReverb => _isReverb ??= GetIsReverb();

  private bool GetIsDelay() {
    // "Buzz" is Analog Tape Delay!
    return EffectType is "Buzz" or "DiffuseDelay" or "DualDelay" or "DualDelayX" 
      or "FatDelay" or "FxDelay" or "PingPongDelay" or "SimpleDelay" or "StereoDelay" 
      or "TapeEcho" or "TrackDelay" or "VelvetDelay";
  }

  private bool GetIsReverb() {
    // SampledReverb is IReverb.
    // DelayedReverb is PreDelay Verb
    return EffectType is "DelayedReverb" or "Diffusion" or "FilteredReverb" 
      or "GateReverb" or "PlainReverb" or "SampledReverb" 
      or "SimpleReverb" or "SparkVerb" or "TapeEcho";
  }
}