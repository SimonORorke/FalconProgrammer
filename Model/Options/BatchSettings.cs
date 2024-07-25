using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class BatchSettings {
  [XmlElement]
  public BatchScope Scope { get; [ExcludeFromCodeCoverage] set; } = new BatchScope();

  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<string> Tasks { get; [ExcludeFromCodeCoverage] set; } = [];
}