using System.Xml;

namespace FalconProgrammer.Model;

public class SettingsFolderLocationReader
  : XmlReaderBase<SettingsFolderLocation> {
  public virtual ISettingsFolderLocation Read() {
    SettingsFolderLocation? result = null;
    try {
      result = Deserialiser.Deserialise(
        SettingsFolderLocation.GetSettingsFolderLocationPath(AppDataFolderName));
    } catch (XmlException) {
      result = new SettingsFolderLocation();
    }
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