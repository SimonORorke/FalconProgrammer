using System.Xml.Serialization;

namespace FalconProgrammer.XmlDeserialised; 

public class FalconProgram {
  [XmlAttribute] public string DisplayName { get; set; } = null!;
  [XmlAttribute] public string ProgramPath { get; set; } = null!;
  
  [XmlArray("ControlSignalSources")]
  [XmlArrayItem("ConstantModulation")]
  public List<ConstantModulation> ConstantModulations { get; set; } = null!;
  
  [XmlArray("EventProcessors")]
  [XmlArrayItem("ScriptProcessor")]
  public List<ScriptProcessor> ScriptProcessors { get; set; } = null!;
}