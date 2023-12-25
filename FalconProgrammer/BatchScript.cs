using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer;

[XmlRoot("Batch")]
public class BatchScript {
  [XmlArray(nameof(Tasks))]
  [XmlArrayItem("Task")]
  public List<BatchTask> Tasks { get; set; } = [];
  
  [PublicAPI] [XmlIgnore] public string BatchScriptPath { get; set; } = string.Empty;

  public static BatchScript Read(string batchScriptPath) {
    var batchScriptFile = new FileInfo(batchScriptPath);
    string parameterFullPath = batchScriptFile.FullName;
    if (!batchScriptFile.Exists) {
     string combinedBatchScriptPath = Path.Combine(
       Batch.GetBatchFolder().FullName, Path.GetFileName(batchScriptPath));
     batchScriptFile = new FileInfo(combinedBatchScriptPath);
    }
    if (!batchScriptFile.Exists) {
      throw new ApplicationException(
        $"Batch script file '{batchScriptFile.Name}' cannot be found. " + 
        $"Looked for both '{parameterFullPath}' and '{batchScriptFile.FullName}'.");
    }
    using var reader = new StreamReader(batchScriptFile.FullName);
    var serializer = new XmlSerializer(typeof(BatchScript));
    var result = (BatchScript)serializer.Deserialize(reader)!;
    result.BatchScriptPath = batchScriptFile.FullName;
    return result;
  }

  public void Write() {
    var serializer = new XmlSerializer(typeof(BatchScript));
    using var writer = new StreamWriter(BatchScriptPath);
    serializer.Serialize(writer, this);
  }

  public class BatchTask {
    [XmlAttribute] public string Name { get; set; } = string.Empty;
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
    [XmlAttribute] public string Program { get; set; } = string.Empty;
    [XmlArray(nameof(Parameters))]
    [XmlArrayItem("Parameter")]
    public List<BatchTaskParameter> Parameters { get; set; } = [];
  }

  public class BatchTaskParameter {
    [XmlAttribute] public string Name { get; set; } = string.Empty;
    [XmlAttribute] public string Value { get; set; } = string.Empty;
  }
}