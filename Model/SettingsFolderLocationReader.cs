using System.Xml;

namespace FalconProgrammer.Model;

public class SettingsFolderLocationReader
  : XmlReaderBase<SettingsFolderLocation> {
  /// <summary>
  ///   Reads the location of the settings folder from a little XML file in the
  ///   application's app data folder (~/Library in macOS). 
  /// </summary>
  public virtual ISettingsFolderLocation Read() {
    SettingsFolderLocation? result = null;
    try {
      result = Deserialiser.Deserialise(
        SettingsFolderLocation.GetSettingsFolderLocationPath(AppDataFolderName));
    } catch (XmlException) {
      // Ideally, we would like to report the XML error in the settings folder location
      // file to the user, as we do when there is an XML error the the settings file.
      // In the case of a settings folder location file error, that would require code
      // to stop the error message box being shown multiple times when the user attempts
      // to change tab pages and gets sent back to the Locations page.
      //
      // I decided it's not worth the effort, as the user can very easily fix the problem
      // by selecting a settings folder, whose path will be saved as valid XML to the
      // settings folder location file.
      //
      // So, instead of reporting the error, we just provide an empty settings folder
      // path.
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