namespace FalconProgrammer.Model;

public class SettingsFolderLocationReader(
  IFileSystemService fileSystemService,
  ISerialiser serialiser,
  string applicationName = Global.ApplicationName)
  : DeserialiserBase<SettingsFolderLocation>(
    fileSystemService, serialiser, applicationName) {
  public SettingsFolderLocation Read() {
    var result = Deserialise(
      SettingsFolderLocation.GetSettingsFolderLocationPath(ApplicationName));
    if (!string.IsNullOrWhiteSpace(result.Path)) {
      FileSystemService.CreateFolder(result.Path);
    }
    return result;
  }
}