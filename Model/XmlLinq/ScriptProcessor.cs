using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using FalconProgrammer.Model.Options;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessor : ModulationsOwner {
  private ScriptId? _scriptId;
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
  ///   Gets the Pascal-style category name from <see cref="Script" />.
  ///   Currently only applies to the Pulsar sound bank, otherwise null.
  ///   Pulsar example: 'Bass' from 'categorie = "bass"...'
  /// </summary>
  /// <remarks>
  ///   For the Pulsar sound bank, the GUI Script process modulation destinations
  ///   vary between categories. So category-specific templates are required.
  ///   <para>
  ///     The Organic Pads sound bank also includes a category parameter in
  ///     <see cref="Script" />. However, it only affects the appearance of the GUI,
  ///     not modulations. Therefore the same template can be uses for al categories.
  ///     So the Organic Pads CategoryId is not of interest and will be null.
  ///   </para>
  /// </remarks>
  public string? CategoryPascal {
    get {
      if (SoundBankId != SoundBankId.Pulsar) {
        return null;
      }
      string scriptWithoutPrefix = Script[13..];
      string categoryLowerCase = scriptWithoutPrefix[..scriptWithoutPrefix.IndexOf('"')];
      string categoryUpperCase = string.Concat(
        categoryLowerCase[0].ToString().ToUpper(), categoryLowerCase.AsSpan(1));
      return categoryUpperCase;
    }
  }

  /// <summary>
  ///   Only needed when a GUI script processor's MIDI CC numbers are assigned.
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
  public ScriptId ScriptId => _scriptId ??= GetScriptId();
  public string ScriptPath => GetAttributeValue(PropertiesElement, nameof(ScriptPath));
  public SoundBankId SoundBankId => Global.GetEnumValue<SoundBankId>(SoundBankPascal);

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
    if (ScriptId == ScriptId.Factory2_1) {
      // Falcon Factory\Brutal Bass 2.1
      newModulation.FixToggleOrContinuous(Macros!, Modulations);
    }
    base.AddModulation(newModulation);
  }

  public void AssignMacroCcsFromTemplate(
    IList<Modulation> templateModulations,
    IList<Macro> macros) {
    Macros = macros;
    if (ScriptId == ScriptId.FactoryRev2) {
      // This code is not used at present, as assigning MIDI CC numbers to macros on a
      // script-based Info page is not supported for the Falcon Factory rev2 sound bank.
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

  /// <summary>
  ///   This code is not used at present, as assigning MIDI CC numbers to macros on a
  ///   script-based Info page is not supported for the Falcon Factory rev2 sound bank.
  /// </summary>
  [ExcludeFromCodeCoverage]
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
    XElement scriptProcessorElement, ProgramXml programXml, MidiForMacros midi, 
    bool mustUseGuiScriptProcessor) {
    var candidate = new ScriptProcessor(scriptProcessorElement, programXml, midi);
    if (candidate.ScriptId == ScriptId.Mpe) {
      return new MpeScriptProcessor(scriptProcessorElement, programXml, midi);
    }
    if (mustUseGuiScriptProcessor) {
      return soundBankId switch {
        SoundBankId.OrganicKeys => new OrganicGuiScriptProcessor(
          scriptProcessorElement, programXml, midi),
        SoundBankId.OrganicPads => new OrganicPadsGuiScriptProcessor(
          scriptProcessorElement, programXml, midi),
        _ => new ScriptProcessor(scriptProcessorElement, programXml, midi)
      };
    }
    return candidate;
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

  private ScriptId GetScriptId() {
    // The CDATA wrapper is stripped off in Script.
    // Example: instead of <![CDATA[require("Factory2_1")]]>, require("Factory2_1").
    // Also, some sound banks (including Organic Pads, Pulsar, Titanium) start the CDATA
    // with a category or colour parameter.
    // Example: <![CDATA[category = "Dark"; require "OrganicPads"]]>
    // So we parse Script with EndWith.
    if (SoundBankId == SoundBankId.FactoryRev2 &&
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
    if (ScriptPath.EndsWith("/MPE.lua")) {
      return ScriptId.Mpe;
    }
    return Script.EndsWith("require \"OrganicTexture\"")
      ? ScriptId.OrganicTexture
      : ScriptId.None;
  }

  public void Remove() {
    var parent = Element.Parent!;
    Element.Remove();
    if (!parent.HasElements) {
      parent!.Remove();
    }
  }
}