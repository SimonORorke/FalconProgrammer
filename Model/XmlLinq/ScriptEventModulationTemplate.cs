using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptEventModulationTemplate : EmbeddedTemplate  {
  public ScriptEventModulationTemplate() :
    base("ScriptEventModulationTemplate.xml") { }
  
  public XElement ScriptEventModulationElement => 
    RootElement.Elements("ScriptEventModulation").First();
}