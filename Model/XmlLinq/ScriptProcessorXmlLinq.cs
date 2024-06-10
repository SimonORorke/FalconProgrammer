using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessorXmlLinq : EmbeddedXmlLinq {
  public ScriptProcessorXmlLinq(string embeddedFileName) :
    base(embeddedFileName) { }
  
  public XElement ScriptProcessorElement => 
    RootElement.Elements("ScriptProcessor").First();
}