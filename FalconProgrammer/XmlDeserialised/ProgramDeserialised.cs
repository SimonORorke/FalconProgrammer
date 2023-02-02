using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised; 

public class ProgramDeserialised {
  [XmlAttribute] public string DisplayName { get; set; } = null!;
  [XmlAttribute] public string ProgramPath { get; set; } = null!;
  
  [XmlArray("ControlSignalSources")]
  [XmlArrayItem("ConstantModulation")]
  public List<Macro> Macros { get; set; } = null!;
  
  [XmlArray("EventProcessors")]
  [XmlArrayItem("ScriptProcessor")]
  public List<ScriptProcessor> ScriptProcessors { get; set; } = null!;
}