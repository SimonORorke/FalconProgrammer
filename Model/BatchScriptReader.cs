using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public class BatchScriptReader : XmlReaderBase<BatchScript> {

  public BatchScript Read(string batchScriptPath) {
    if (!FileSystemService.File.Exists(batchScriptPath)) {
      throw new ApplicationException(
        $"Batch script file '{batchScriptPath}' cannot be found.");
    }
    using var reader = new StreamReader(batchScriptPath);
    var serializer = new XmlSerializer(typeof(BatchScript));
    var result = (BatchScript)serializer.Deserialize(reader)!;
    result.BatchScriptPath = batchScriptPath;
    return result;
  }
}