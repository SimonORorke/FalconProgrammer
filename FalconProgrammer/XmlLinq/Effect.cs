using System.Collections.Immutable;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class Effect : INamed {
  private bool? _isDelay;
  private bool? _isReverb;

  // private ImmutableList<Macro>? _modulatingMacros;
  private ImmutableList<SignalConnection>? _signalConnections;

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
  public bool IsModulated => SignalConnections.Count > 0;

  // public ImmutableList<Macro> ModulatingMacros {
  //   get => _modulatingMacros ??= GetModulatingMacros();
  //   set => _modulatingMacros = value;
  // }
  private ProgramXml ProgramXml { get; }

  /// <summary>
  ///   SignalConnections specifying modulators of the effect.
  /// </summary>
  public ImmutableList<SignalConnection> SignalConnections {
    get => _signalConnections ??= GetSignalConnections();
    private set => _signalConnections = value;
  }

  public string Name { get; }

  /// <summary>
  ///   TODO: Review ChangeModulatedParametersToZero requirement
  /// </summary>
  public void ChangeModulatedParametersToZero() {
    foreach (var signalConnection in SignalConnections) {
      try {
        ProgramXml.SetAttribute(
          EffectElement, signalConnection.Destination,
          // If it's a toggle macro, Destination should be "Bypass".  
          signalConnection.Destination == "Bypass" ? 1 : 0);
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

  private ImmutableList<SignalConnection> GetSignalConnections() {
    var list = new List<SignalConnection>();
    var connectionsElement = EffectElement.Element("Connections");
    if (connectionsElement != null) {
      list.AddRange(connectionsElement.Elements("SignalConnection").Select(
        signalConnectionElement => new SignalConnection(
          this, signalConnectionElement)));
    }
    return list.ToImmutableList();
  }

  public void RemoveModulationsByMacro(Macro macro) {
    var signalConnections = SignalConnections.ToList();
    for (int i = signalConnections.Count - 1; i >= 0; i--) {
      if (signalConnections[i].SourceMacro == macro) {
        signalConnections[i].SignalConnectionElement!.Remove();
        signalConnections.Remove(signalConnections[i]);
      }
    }
    SignalConnections = signalConnections.ToImmutableList();
    var connectionsElement = EffectElement.Element("Connections");
    if (connectionsElement != null && !connectionsElement.Nodes().Any()) {
      connectionsElement.Remove();
    }
  }
}