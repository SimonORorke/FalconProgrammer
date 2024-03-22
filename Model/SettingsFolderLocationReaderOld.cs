namespace FalconProgrammer.Model;

public class SettingsFolderLocationReaderOld
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