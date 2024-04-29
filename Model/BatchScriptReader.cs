namespace FalconProgrammer.Model;

public class BatchScriptReader : XmlReaderBase<BatchScript> {

  public virtual BatchScript Read(string batchScriptPath) {
    if (!FileSystemService.File.Exists(batchScriptPath)) {
      throw new ApplicationException(
        $"Batch script file '{batchScriptPath}' cannot be found.");
    }
    return Deserialiser.Deserialise(batchScriptPath);
  }
}