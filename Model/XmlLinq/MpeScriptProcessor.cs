using System.Xml.Linq;
using FalconProgrammer.Model.Mpe;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   MPE <see cref="ScriptProcessor" />,
///   for configuration of MIDI/Multidimensional Polyphonic Expression.
/// </summary>
internal class MpeScriptProcessor : ScriptProcessor {
  public MpeScriptProcessor(ProgramXml programXml,
    MidiForMacros midi) : base(
    programXml.AddScriptProcessorElementFromTemplate("MpeScriptProcessor.xml"),
    programXml, midi) { }

  protected internal MpeScriptProcessor(
    XElement scriptProcessorElement, ProgramXml programXml, MidiForMacros midi) : base(
    scriptProcessorElement, programXml, midi) { }

  public XTarget XTarget {
    get => (XTarget)Convert.ToInt32(GetAttributeValue("X"));
    private set => SetAttribute("X", (int)value);
  }

  public YTarget YTarget {
    get => (YTarget)Convert.ToInt32(GetAttributeValue("Y"));
    private set => SetAttribute("Y", (int)value);
  }

  public ZTarget ZTarget {
    get => (ZTarget)Convert.ToInt32(GetAttributeValue("Z"));
    private set => SetAttribute("Z", (int)value);
  }

  public void Configure(IList<Macro> macrosToEmulate, MpeSettings mpeSettings) {
    YTarget =
      mpeSettings.YTargetValue is YTarget.ContinuousMacro1Bipolar
        or YTarget.ContinuousMacro1Unipolar
        ? macrosToEmulate.Count >= 1
          ? mpeSettings.YTargetValue
          : YTarget.PolyphonicAftertouch
        : mpeSettings.YTargetValue;
    ZTarget =
      mpeSettings.ZTargetValue is ZTarget.ContinuousMacro2Bipolar
        or ZTarget.ContinuousMacro2Unipolar
        ? macrosToEmulate.Count >= 2 ? mpeSettings.ZTargetValue : ZTarget.Gain
        : mpeSettings.ZTargetValue;
    XTarget =
      mpeSettings.XTargetValue is XTarget.ContinuousMacro3Bipolar
        ? macrosToEmulate.Count >= 3 ? mpeSettings.XTargetValue : XTarget.Pitch
        : mpeSettings.XTargetValue;
    List<ScriptEventModulation> dimensionModulations = [];
    switch (YTarget) {
      case YTarget.ContinuousMacro1Bipolar:
      case YTarget.ContinuousMacro1Unipolar:
        dimensionModulations.Add(
          CreateDimensionModulation(MpeEventId.Y, YTarget.ToString()));
        break;
    }
    switch (ZTarget) {
      case ZTarget.ContinuousMacro2Bipolar:
      case ZTarget.ContinuousMacro2Unipolar:
        dimensionModulations.Add(
          CreateDimensionModulation(MpeEventId.Z, ZTarget.ToString()));
        break;
    }
    switch (XTarget) {
      case XTarget.ContinuousMacro3Bipolar:
        dimensionModulations.Add(
          CreateDimensionModulation(MpeEventId.X, XTarget.ToString()));
        break;
    }
    for (int i = 0; i < dimensionModulations.Count; i++) {
      EmulateMacroWithDimension(macrosToEmulate[i], dimensionModulations[i]);
    }
  }

  private ScriptEventModulation CreateDimensionModulation(
    MpeEventId eventId, string targetName) {
    var result = new ScriptEventModulation(ProgramXml) {
      Name = $"MPE {eventId} Modulation",
      Bipolar = targetName.EndsWith("Bipolar"),
      EventId = (int)eventId
    };
    result.DisplayName = result.Name;
    return result;
  }

  /// <summary>
  ///   For each modulation of an effect by the specified macro,
  ///   add the same kind of modulation of the same effect by the specified
  ///   MPE dimension.
  /// </summary>
  /// <remarks>
  ///   Modulating a <see cref="Macro" /> with a <see cref="ScriptEventModulation" />
  ///   does not work, at least when controlled by an MPE <see cref="ScriptProcessor" />.
  ///   So this is a workaround. It has the disadvantage that the macro's value won't
  ///   get changed when the MPE dimension's value is changed.
  /// </remarks>
  private void EmulateMacroWithDimension(
    Macro macro, ScriptEventModulation dimensionModulation) {
    foreach (var connectionsParent in macro.ModulatedConnectionsParents) {
      var modulationsByMacro =
        from modulation in connectionsParent.Modulations
        where modulation.Source.EndsWith(macro.Name)
        select modulation;
      foreach (var modulationByMacro in modulationsByMacro) {
        var modulationByDimension = new Modulation(ProgramXml) {
          Name = dimensionModulation.Name,
          Ratio = modulationByMacro.Ratio,
          Source = $"$Program/{dimensionModulation.Name}",
          Destination = modulationByMacro.Destination,
          Owner = connectionsParent
        };
        connectionsParent.AddModulation(modulationByDimension);
      }
    }
  }

  public static bool Exists(IList<ScriptProcessor> scriptProcessors) {
    return (
      from scriptProcessor in scriptProcessors
      where scriptProcessor is MpeScriptProcessor
      select scriptProcessor).Any();
  }
}