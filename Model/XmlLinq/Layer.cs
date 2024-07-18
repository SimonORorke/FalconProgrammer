using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class Layer : ModulationsOwner {
  public Layer(XElement element, ProgramXml programXml, MidiForMacros midi) : base(
    element, programXml, midi) { }

  public float Gain {
    get => Convert.ToSingle(GetAttributeValue(nameof(Gain)));
    set => SetAttribute(nameof(Gain), value);
  }
}