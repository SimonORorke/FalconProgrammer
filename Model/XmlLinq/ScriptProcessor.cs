using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessor : ModulationsOwner {
  private ScriptId? _guiScriptId;
  private XElement? _propertiesElement;
  private XElement? _scriptElement;

  /// <summary>
  ///   Use the <see cref="Create" /> static method for public instantiation of the
  ///   correct type of <see cref="ScriptProcessor" />.
  /// </summary>
  protected ScriptProcessor(XElement scriptProcessorElement, ProgramXml programXml,
    MidiForMacros midi) : base(programXml, midi) {
    Element = scriptProcessorElement;
  }

  /// <summary>
  ///   Gets the category name from <see cref="Script" />.
  ///   Currently only applies to the Pulsar sound bank, otherwise null.
  ///   Example: 'Bass' from 'categorie = "bass"...'
  /// </summary>
  /// <remarks>
  ///   For the Pulsar sound bank, the GUI Script process modulation destinations
  ///   vary between categories. So category-specific templates are required.
  ///   <para>
  ///     The Organic Pads sound bank also includes a category parameter in
  ///     <see cref="Script" />. However, it only affects the appearance of the GUI,
  ///     not modulations. Therefore the same template can be uses for al categories.
  ///     So the Organic Pads Category is not of interest and will be null.
  ///   </para>
  /// </remarks>
  public string? Category {
    get {
      if (SoundBankId != SoundBankId.Pulsar) {
        return null;
      }
      string scriptWithoutPrefix = Script[13..];
      string lowerCaseCategory = scriptWithoutPrefix[..scriptWithoutPrefix.IndexOf('"')];
      string result = string.Concat(
        lowerCaseCategory[0].ToString().ToUpper(), lowerCaseCategory.AsSpan(1));
      return result;
    }
  }

  public ScriptId GuiScriptId => _guiScriptId ??= GetGuiScriptId();

  /// <summary>
  ///   Only needed when a GUI script processor's MIDI CC numbers are updated.
  /// </summary>
  private IList<Macro>? Macros { get; set; }

  private XElement PropertiesElement => _propertiesElement ??= GetPropertiesElement();

  /// <summary>
  ///   Gets the script specification, which comes with the CDATA wrapper in the file
  ///   stripped off.
  ///   Example: instead of <![CDATA[require("Factory2_1")]]>, require("Factory2_1").
  /// </summary>
  [PublicAPI]
  public string Script => ScriptElement.Value;

  private XElement ScriptElement => _scriptElement ??= GetScriptElement();
  public string ScriptPath => GetAttributeValue(PropertiesElement, nameof(ScriptPath));
  public SoundBankId SoundBankId => Global.GetSoundBankId(SoundBankPascal);

  /// <summary>
  ///   Gets the Pascal-style (spaces removed) sound bank identifier from
  ///   <see cref="ScriptPath" />.
  ///   Example: "FalconFactory" from "$Falcon Factory.ufs/Scripts/Factory2_5_Stub.lua".
  /// </summary>
  public string SoundBankPascal {
    get {
      if (!ScriptPath.StartsWith('$') || !ScriptPath.Contains('.')) {
        // OrganicPads_DahdsrController.xml is currently the only one.
        return string.Empty;
      }
      return ScriptPath.StartsWith("$Falcon Factory rev2")
        ? "FactoryRev2"
        : ScriptPath[..ScriptPath.IndexOf('.')][1..].Replace(
          " ", string.Empty);
    }
  }

  public override void AddModulation(Modulation templateModulation) {
    // Clone the template modulation, to guard against updating it.
    var newModulation = new Modulation(this,
      new XElement(templateModulation.Element), ProgramXml, Midi);
    if (GuiScriptId == ScriptId.Factory2_1) {
      // Falcon Factory\Brutal Bass 2.1
      newModulation.FixToggleOrContinuous(Macros!, Modulations);
    }
    base.AddModulation(newModulation);
  }

  private void AddModulationBasedOnMacro(
    Modulation templateModulation, Macro correspondingMacro) {
    int newCcNo = correspondingMacro.IsContinuous
      ? Midi.GetNextContinuousCcNo(false)
      : Midi.GetNextToggleCcNo();
    templateModulation.Source =
      templateModulation.Source.Replace(templateModulation.CcNo!.Value.ToString(),
        newCcNo.ToString());
    AddModulation(templateModulation);
  }

  public static ScriptProcessor Create(SoundBankId soundBankId,
    XElement scriptProcessorElement, ProgramXml programXml, MidiForMacros midi) {
    return soundBankId switch {
      SoundBankId.OrganicKeys => new OrganicKeysScriptProcessor(
        scriptProcessorElement, programXml, midi),
      _ => new ScriptProcessor(scriptProcessorElement, programXml, midi)
    };
  }

  private ScriptId GetGuiScriptId() {
    // The CDATA wrapper is stripped off in Script.
    // Example: instead of <![CDATA[require("Factory2_1")]]>, require("Factory2_1").
    // Also, some sound banks (including Organic Pads, Pulsar, Titanium) start the CDATA
    // with a category or colour parameter.
    // Example: <![CDATA[category = "Dark"; require "OrganicPads"]]>
    // So we parse Script with EndWith.
    if (SoundBankPascal == "FactoryRev2" &&
        (Script.EndsWith("require 'FalconFactory'") ||
         Script.EndsWith("require \"FalconFactory\""))) {
      return ScriptId.FactoryRev2;
    }
    if (Script.EndsWith($"require \"{SoundBankPascal}\"")) {
      return ScriptId.SoundBank1;
    }
    if (Script.EndsWith($"require(\"{SoundBankPascal}\")")) {
      return ScriptId.SoundBank2;
    }
    if (Script.EndsWith("require(\"Factory2_1\")")) {
      return ScriptId.Factory2_1;
    }
    if (Script.EndsWith("require 'Factory2_5'")) {
      return ScriptId.Factory2_5;
    }
    if (Script.EndsWith("require(\"main\")")) {
      return ScriptId.Main1;
    }
    if (Script.EndsWith("require \"main\"")) {
      return ScriptId.Main2;
    }
    return Script.EndsWith("require \"OrganicTexture\"")
      ? ScriptId.OrganicTexture
      : ScriptId.None;
  }

  private XElement GetPropertiesElement() {
    var result = Element.Element("Properties");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ScriptProcessor.Properties element.");
    }
    return result;
  }

  private XElement GetScriptElement() {
    var result = Element.Element("script");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ScriptProcessor.script element.");
    }
    return result;
  }

  public void Remove() {
    var eventProcessorsElement = Element.Parent;
    // We need to remove the EventProcessors element, including all its
    // ScriptProcessor elements, if there are more than one.
    // Just removing the Info page CCs ScriptProcessor element will not work.
    // ReSharper disable once CommentTypo
    // Example: Falcon Factory\RetroWave 2.5\BAS Voltage Reso.
    eventProcessorsElement!.Remove();
  }

  public void UpdateModulationsFromTemplate(
    IList<Modulation> templateModulations,
    IList<Macro> macros) {
    Macros = macros;
    if (GuiScriptId == ScriptId.FactoryRev2) {
      Midi.CurrentContinuousCcNo = 0;
      Midi.CurrentToggleCcNo = 0;
      if (templateModulations.Count >= macros.Count) {
        // This does not always accurately distinguish the continuous macros from
        // the toggle macros.
        // Example of where it does not work: Falcon Factory rev2\Bass\Big Sleep.
        // But it seems to be more successful than having the macros in location order.
        for (int i = 0; i < macros.Count; i++) {
          AddModulationBasedOnMacro(templateModulations[i], macros[i]);
        }
      } else {
        throw new ApplicationException(
          "There are more macros than template modulations.");
      }
    } else {
      foreach (var templateModulation in templateModulations) {
        AddModulation(templateModulation);
      }
    }
  }
}