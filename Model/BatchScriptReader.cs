namespace FalconProgrammer.Model;

public class BatchScriptReader : XmlReaderBase<BatchScript> {
  public BatchScript Read(string batchScriptPath) {
    if (!FileSystemService.File.Exists(batchScriptPath)) {
      throw new ApplicationException(
        $"Batch script file '{batchScriptPath}' cannot be found.");
    }
    Deserialiser.AppDataFolderName = AppDataFolderName;
    Deserialiser.FileSystemService = FileSystemService;
    Deserialiser.Serialiser = Serialiser;
    var result = Deserialiser.Deserialise(batchScriptPath);
    result.Path = batchScriptPath;
    return result;
  }
}