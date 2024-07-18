using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptEventModulation : EntityBase {
  public ScriptEventModulation(ProgramXml programXml) : base(
    programXml, true) { }

  public bool Bipolar {
    get => GetAttributeValue(nameof(Bipolar)) == "1";
    set => SetAttribute(nameof(Bipolar), value ? "1" : "0");
  }

  public int EventId {
    get => Convert.ToInt32(GetAttributeValue(nameof(EventId)));
    set => SetAttribute(nameof(EventId), value);
  }

  protected override XElement CreateElementFromTemplate() {
    var result = new XElement(
      new ScriptEventModulationTemplate().ScriptEventModulationElement);
    ProgramXml.ControlSignalSourcesElement.Add(result);
    return result;
  }
}