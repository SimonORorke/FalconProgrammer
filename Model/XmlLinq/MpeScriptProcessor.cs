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

  private void EmulateMacroWithDimension(
    Macro macro, ScriptEventModulation dimensionModulation) {
    foreach (var connectionsParent in macro.ModulatedConnectionsParents) {
      foreach (var modulation in connectionsParent.Modulations) {
        // modulation.SourceMacro = (
        //   from macro in Macros
        //   where modulation.Source.EndsWith(macro.Name)
        //   select macro).FirstOrDefault();
        // modulation.SourceMacro?.ModulatedConnectionsParents.Add(connectionsParent);
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