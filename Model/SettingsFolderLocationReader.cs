namespace FalconProgrammer.Model;

public class SettingsFolderLocationReader
  : XmlReaderBase<SettingsFolderLocation> {
  public SettingsFolderLocation Read() {
    var result = Deserialiser.Deserialise(
      SettingsFolderLocation.GetSettingsFolderLocationPath(ApplicationName));
    if (!string.IsNullOrWhiteSpace(result.Path)) {
      FileSystemService.Folder.Create(result.Path);
    }
    return result;
  }
}