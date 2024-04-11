namespace FalconProgrammer.Model;

public class SettingsFolderLocationReader
  : XmlReaderBase<SettingsFolderLocation> {
  public virtual ISettingsFolderLocation Read() {
    var result = Deserialiser.Deserialise(
      SettingsFolderLocation.GetSettingsFolderLocationPath(ApplicationName));
    if (!string.IsNullOrWhiteSpace(result.Path)) {
      try {
        FileSystemService.Folder.Create(result.Path);
      } catch (DirectoryNotFoundException) {
        // Invalid parent folder
      }
    }
    return result;
  }
}