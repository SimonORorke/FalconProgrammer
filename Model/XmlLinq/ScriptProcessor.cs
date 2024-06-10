using System.Xml.Linq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessor : ModulationsOwner {
  public enum ScriptId {
    None,

    /// <summary>
    ///   Works for Falcon Factory\Brutal Bass 2.1.
    /// </summary>
    Factory2_1,

    /// <summary>
    ///   Works for Falcon Factory categories Lo-Fi 2.5, RetroWave 2.5,
    ///   VCF-20 Synths 2.5.
    /// </summary>
    Factory2_5,

    /// <summary>
    ///   Works for Pulsar, Savage, Voklm\Vox Instruments.
    /// </summary>
    Main1,

    /// <summary>
    ///   Works for Voklm\Synth Choirs.
    /// </summary>
    Main2,

    /// <summary>
    ///   Works for Falcon Factory\Organic Texture 2.8.
    /// </summary>
    OrganicTexture,

    /// <summary>
    ///   Works for Fluidity, Hypnotic Drive, Inner Dimensions, Modular Noise,
    ///   Organic Keys, Organic Pads.
    /// </summary>
    Standard1,

    /// <summary>
    ///   Works for Titanium.
    /// </summary>
    Standard2
  }

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
  [PublicAPI] public string Script => ScriptElement.Value;

  private XElement ScriptElement => _scriptElement ??= GetScriptElement();
  public string ScriptPath => GetAttributeValue(PropertiesElement, nameof(ScriptPath));

  /// <summary>
  ///   Gets the Pascal-style (spaces removed) sound bank identifier from
  ///   <see cref="ScriptPath" />.
  ///   Example: "FalconFactory" from "$Falcon Factory.ufs/Scripts/Factory2_5_Stub.lua".
  /// </summary>
  public string SoundBankId =>
    ScriptPath[..ScriptPath.IndexOf('.')][1..].Replace(" ", string.Empty);

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

  public static ScriptProcessor Create(string soundBankName,
    XElement scriptProcessorElement, ProgramXml programXml, MidiForMacros midi) {
    return soundBankName switch {
      "Organic Keys" => new OrganicKeysScriptProcessor(
        scriptProcessorElement, programXml, midi),
      _ => new ScriptProcessor(scriptProcessorElement, programXml, midi)
    };
  }

  private ScriptId GetGuiScriptId() {
    // The CDATA wrapper is stripped off in ScriptProcessor.Script.
    // Example: instead of <![CDATA[require("Factory2_1")]]>, require("Factory2_1").
    // Also, some sound banks (including Organic Pads, Pulsar, Titanium) start the CDATA
    // with a category or colour parameter.
    // Example: <![CDATA[category = "Dark"; require "OrganicPads"]]>
    // So we parse Script with EndWith.
    if (Script.EndsWith($"require \"{SoundBankId}\"")) {
      return ScriptId.Standard1;
    }
    if (Script.EndsWith($"require(\"{SoundBankId}\")")) {
      return ScriptId.Standard2;
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
        "Cannot find ScriptProcessor.Properties "
        + $"element in '{ProgramXml.Category.TemplateProgramPath}'.");
    }
    return result;
  }

  private XElement GetScriptElement() {
    var result = Element.Element("script");
    if (result == null) {
      throw new InvalidOperationException(
        "Cannot find ScriptProcessor.script "
        + $"element in '{ProgramXml.Category.TemplateProgramPath}'.");
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
    IEnumerable<Modulation> templateModulations, IList<Macro> macros) {
    Macros = macros;
    foreach (var templateModulation in templateModulations) {
      AddModulation(templateModulation);
    }
  }
}