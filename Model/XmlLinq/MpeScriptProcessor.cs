using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model.Mpe;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   MPE <see cref="ScriptProcessor" />,
///   for configuration of MIDI Polyphonic Expression.
/// </summary>
internal class MpeScriptProcessor : ScriptProcessor {
  public MpeScriptProcessor(ProgramXml programXml,
    MidiForMacros midi) : base(
    programXml.AddScriptProcessorElementFromTemplate("MpeScriptProcessor.xml"),
    programXml, midi) { }

  private XTarget XTarget {
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    get => (XTarget)Convert.ToInt32(GetAttributeValue("X"));
    set => SetAttribute("X", (int)value);
  }

  private YTarget YTarget {
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    get => (YTarget)Convert.ToInt32(GetAttributeValue("Y"));
    set => SetAttribute("Y", (int)value);
  }

  private ZTarget ZTarget {
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    get => (ZTarget)Convert.ToInt32(GetAttributeValue("Z"));
    set => SetAttribute("Z", (int)value);
  }

  public void Configure(IList<Macro> macrosToEmulate) {
    List<ScriptEventModulation> dimensionModulations = [];
    if (macrosToEmulate.Count >= 3) {
      XTarget = XTarget.ScriptEventMod2Binary;
      dimensionModulations.Add(new ScriptEventModulation(ProgramXml) {
        Name = "MPE X Modulation",
        DisplayName = Name,
        Bipolar = true,
        EventId = 2
      });
    } else {
      XTarget = XTarget.Pitch;
    }
    if (macrosToEmulate.Count >= 2) {
      ZTarget = ZTarget.ScriptEventMod0Binary;
      dimensionModulations.Add(new ScriptEventModulation(ProgramXml) {
        Name = "MPE Z Modulation",
        DisplayName = Name,
        Bipolar = true,
        EventId = 0
      });
    } else {
      ZTarget = ZTarget.Gain;
    }
    if (macrosToEmulate.Count >= 1) {
      YTarget = YTarget.ScriptEventMod1Binary;
      dimensionModulations.Add(new ScriptEventModulation(ProgramXml) {
        Name = "MPE Y Modulation",
        DisplayName = Name,
        Bipolar = true,
        EventId = 1
      });
    } else {
      YTarget = YTarget.PolyphonicAftertouch;
    }
    for (int i = 0; i < dimensionModulations.Count; i++) {
      EmulateMacroWithDimension(macrosToEmulate[i], dimensionModulations[i]);
    }
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
      where scriptProcessor.ScriptId == ScriptId.Mpe
      select scriptProcessor).Any();
  }
}