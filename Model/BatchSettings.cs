using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public class BatchSettings {
  [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
  [XmlAttribute] public string Category { get; set; } = string.Empty;
  [XmlAttribute] public string Program { get; set; } = string.Empty;

  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<string> Tasks {
    get;
    [ExcludeFromCodeCoverage] set;
  } = [];
}