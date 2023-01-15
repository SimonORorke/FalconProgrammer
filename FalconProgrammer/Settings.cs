using System.Xml.Serialization;
using JetBrains.Annotations;

namespace FalconProgrammer; 

[XmlRoot(nameof(Settings))] public class Settings {
  [PublicAPI] public string DefaultSettingsFolderPath { get; set; } =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\FalconProgrammer Settings";

  [XmlArray("ProgramTemplates")]
  [XmlArrayItem(nameof(ProgramTemplate))]
  public List<ProgramTemplate> ProgramTemplates { get; set; } = null!;
  
  public class ProgramTemplate {
    [XmlAttribute] public string Path { get; set; } = null!;
  }
}