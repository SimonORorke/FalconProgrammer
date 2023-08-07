using System.Collections.Immutable;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class Effect : INamed {
  private bool? _isDelay;
  private bool? _isReverb;

  private ImmutableList<Modulation>? _modulations;

  public Effect(XElement effectElement, ProgramXml programXml) {
    EffectElement = effectElement;
    ProgramXml = programXml;
    EffectType = EffectElement.Name.ToString();
    Name = ProgramXml.GetAttributeValue(EffectElement, nameof(Name));
  }

  public bool Bypass {
    get => ProgramXml.GetAttributeValue(EffectElement, "Bypass") == "1";
    set => ProgramXml.SetAttribute(EffectElement, "Bypass", value ? "1" : "0");
  }

  private XElement EffectElement { get; }
  public string EffectType { get; }
  public bool IsDelay => _isDelay ??= GetIsDelay();
  public bool IsReverb => _isReverb ??= GetIsReverb();
  public bool IsModulated => Modulations.Count > 0;

  private ProgramXml ProgramXml { get; }

  /// <summary>
  ///   Modulations specifying modulators of the effect.
  /// </summary>
  public ImmutableList<Modulation> Modulations {
    get => _modulations ??= GetModulations();
    private set => _modulations = value;
  }

  public string Name { get; }

  /// <summary>
  ///   TODO: Review ChangeModulatedParametersToZero requirement
  /// </summary>
  public void ChangeModulatedParametersToZero() {
    foreach (var modulation in Modulations) {
      try {
        ProgramXml.SetAttribute(
          EffectElement, modulation.Destination,
          // If it's a toggle macro, Destination should be "Bypass".  
          modulation.Destination == "Bypass" ? 1 : 0);
        // ReSharper disable once EmptyGeneralCatchClause
      } catch { }
    }
  }

  private bool GetIsDelay() {
    // "Buzz" is Analog Tape Delay!
    return EffectType is "Buzz" or "DualDelay" or "FatDelay" or "FxDelay"
      or "PingPongDelay" or "SimpleDelay" or "StereoDelay" or "TapeEcho";
  }

  private bool GetIsReverb() {
    return EffectType is "DelayedReverb" or "FilteredReverb" or "PlainReverb"
      or "SampledReverb" or "SharpVerb" or "TapeEcho";
  }

  private ImmutableList<Modulation> GetModulations() {
    var list = new List<Modulation>();
    var connectionsElement = EffectElement.Element("Connections");
    if (connectionsElement != null) {
      list.AddRange(connectionsElement.Elements("SignalConnection").Select(
        modulationElement => new Modulation(
          this, modulationElement)));
    }
    return list.ToImmutableList();
  }

  public void RemoveModulationsByMacro(Macro macro) {
    var modulations = Modulations.ToList();
    for (int i = modulations.Count - 1; i >= 0; i--) {
      if (modulations[i].SourceMacro == macro) {
        modulations[i].ModulationElement!.Remove();
        modulations.Remove(modulations[i]);
      }
    }
    Modulations = modulations.ToImmutableList();
    var connectionsElement = EffectElement.Element("Connections");
    if (connectionsElement != null && !connectionsElement.Nodes().Any()) {
      connectionsElement.Remove();
    }
  }
}