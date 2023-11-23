using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

/// <summary>
///   ScriptProcessor for the "Organic Pads" sound bank.
/// </summary>
public class OrganicPadsScriptProcessor : OrganicScriptProcessor {
  /// <summary>
  ///   Use the <see cref="ScriptProcessor.Create" /> static method for public
  ///   instantiation of the correct type of <see cref="ScriptProcessor" />.
  /// </summary>
  public OrganicPadsScriptProcessor(XElement scriptProcessorElement,
    ProgramXml programXml) : base(scriptProcessorElement, programXml) { }

  public int CcX {
    get => Convert.ToInt32(GetAttributeValue(nameof(CcX).ToUpper()));
    set => SetAttribute(nameof(CcX).ToUpper(), value);
  }

  public int CcY {
    get => Convert.ToInt32(GetAttributeValue(nameof(CcY).ToUpper()));
    set => SetAttribute(nameof(CcY).ToUpper(), value);
  }

  public override void UpdateModulationsFromTemplate(
    IEnumerable<Modulation> templateModulations) {
    base.UpdateModulationsFromTemplate(templateModulations);
    // Modulate the X and Y co-ordinates of the XY control with MIDI CC numbers.
    // X (Synthesizer vs Sample) seems more useful than Y (Noise vs Texture).
    // So X is modulated by the rightmost of the four main pedals while Y is modulated
    // by the leftmost.
    CcX = 34;
    CcY = 31;
  }
}