namespace FalconProgrammer.Model;

public class SettingsFolderLocationReader
  : DeserialiserBase<SettingsFolderLocation> {
  public SettingsFolderLocation Read() {
    var result = Deserialise(
      SettingsFolderLocation.GetSettingsFolderLocationPath(ApplicationName));
    if (!string.IsNullOrWhiteSpace(result.Path)) {
      FileSystemService.CreateFolder(result.Path);
    }
    return result;
  }
}