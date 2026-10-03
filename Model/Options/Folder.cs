using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class Folder {
  [XmlAttribute]
  public string Path {
    get;
    set => field = string.IsNullOrWhiteSpace(value)
      ? string.Empty
      : System.IO.Path.TrimEndingDirectorySeparator(value.Trim());
  } = string.Empty;
}