using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessorEmbeddedXmlLinq : EmbeddedXmlLinq {
  public ScriptProcessorEmbeddedXmlLinq(string embeddedFileName) :
    base(embeddedFileName) { }
  
  public XElement ScriptProcessorElement => 
    RootElement.Elements("ScriptProcessor").First();
}