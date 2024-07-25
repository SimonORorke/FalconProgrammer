using System.Xml.Linq;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Model.XmlLinq;

internal class Effect : ConnectionsParent {
  private bool? _isDelay;
  private bool? _isReverb;

  public Effect(XElement element, ProgramXml programXml, MidiForMacros midi)
    : base(element, programXml, midi) {
    EffectType = element.Name.ToString();
  }

  public string EffectType { get; }
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