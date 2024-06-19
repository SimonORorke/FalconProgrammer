using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

/// <summary>
///   GUI ScriptProcessor for the "Organic Pads" sound bank.
/// </summary>
internal class OrganicPadsGuiScriptProcessor : OrganicGuiScriptProcessor {
  public OrganicPadsGuiScriptProcessor(XElement scriptProcessorElement,
    ProgramXml programXml, MidiForMacros midi) : base(scriptProcessorElement, programXml,
    midi) {
    // Remove the hard-coded MIDI CC number assignments 21 and 22 for parameters
    // CCX and CCY respectively, the X and Y co-ordinates of the X-Y control on the GUI.
    // Instead, GUI script processor template OrganicPads_Gui.xml specifies modulations
    // for the Horizontal_Slider and Vertical_Slider parameters.  Those are are shown as
    // sliders on the GUI, and provide an alternative way to vary the
    // X and Y co-ordinates of the X-Y control.
    SetAttribute("CCX", string.Empty);
    SetAttribute("CCY", string.Empty);
  }
}