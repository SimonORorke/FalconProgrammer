using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessorEmbeddedXml : EmbeddedXml {
  public ScriptProcessorEmbeddedXml(string embeddedFileName) :
    base(embeddedFileName) { }
  
  public XElement ScriptProcessorElement => 
    RootElement.Elements("ScriptProcessor").First();
}