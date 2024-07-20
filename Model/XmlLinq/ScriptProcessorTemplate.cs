using System.Xml.Linq;

namespace FalconProgrammer.Model.XmlLinq;

internal class ScriptProcessorTemplate : EmbeddedTemplate {
  public ScriptProcessorTemplate(string embeddedFileName) :
    base(embeddedFileName) { }
  
  public XElement ScriptProcessorElement => 
    RootElement.Elements(nameof(ScriptProcessor)).First();
}