namespace FalconProgrammer.Model;

public class BatchScriptReader : XmlReaderBase<BatchScript> {

  public virtual BatchScript Read(string batchScriptPath) {
    if (!FileSystemService.File.Exists(batchScriptPath)) {
      throw new ApplicationException(
        $"Batch script file '{batchScriptPath}' cannot be found.");
    }
    Deserialiser.AppDataFolderName = AppDataFolderName;
    Deserialiser.FileSystemService = FileSystemService;
    Deserialiser.Serialiser = Serialiser;
    return Deserialiser.Deserialise(batchScriptPath);
  }
}