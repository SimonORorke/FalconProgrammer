using System.Collections.Immutable;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot(nameof(Settings))]
public class Settings {
  public const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  [XmlElement] public Folder BatchScriptsFolder { get; set; } = new Folder();
  [XmlElement] public Folder ProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder OriginalProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder TemplateProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Template DefaultTemplate { get; set; } = new Template();

  [XmlArray("MustUseGuiScriptProcessor")]
  [XmlArrayItem(nameof(ProgramCategory))]
  public List<ProgramCategory> MustUseGuiScriptProcessorCategories { get; set; } = [];

  [XmlElement] public MacrosMidi MidiForMacros { get; set; } = new MacrosMidi();
  [XmlIgnore] public string SettingsPath { get; set; } = string.Empty;

  public static FileInfo GetSettingsFile(string settingsFolderPath) {
    return new FileInfo(Path.Combine(settingsFolderPath, "Settings.xml"));
  }

  public bool MustUseGuiScriptProcessor(
    string soundBankFolderName, string categoryName) {
    bool result = (
      from programCategory in MustUseGuiScriptProcessorCategories
      where programCategory.SoundBank == soundBankFolderName &&
            programCategory.Category == categoryName
      select programCategory).Any();
    if (!result) {
      result = (
        from programCategory in MustUseGuiScriptProcessorCategories
        where programCategory.SoundBank == soundBankFolderName &&
              programCategory.Category == string.Empty
        select programCategory).Any();
    }
    return result;
  }

  public static Settings Read(
    string defaultSettingsFolderPath = DefaultSettingsFolderPath,
    string applicationName = SettingsFolderLocation.DefaultApplicationName) {
    var settingsFolderLocation = SettingsFolderLocation.Read(applicationName);
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = defaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    var settingsFile = GetSettingsFile(settingsFolderLocation.Path);
    if (!settingsFile.Exists) {
      return new Settings { SettingsPath = settingsFile.FullName };
    }
    using var reader = new StreamReader(settingsFile.FullName);
    var serializer = new XmlSerializer(typeof(Settings));
    var result = (Settings)serializer.Deserialize(reader)!;
    result.SettingsPath = settingsFile.FullName;
    return result;
  }

  protected virtual void Serialize() {
    var serializer = new XmlSerializer(typeof(Settings));
    using var writer = new StreamWriter(SettingsPath);
    serializer.Serialize(writer, this);
  }

  public void Write(string? settingsPath = null) {
    if (settingsPath!= null) {
      SettingsPath = settingsPath;
    }
    Serialize();
  }

  public class Folder {
    [XmlAttribute]
    public string Path { get; set; } = string.Empty;
  }

  public struct IntegerRange {
    public int Start;
    public int End;
  }

  public class MacrosMidi {
    private ImmutableList<int>? _continuousCcNos;
    private ImmutableList<int>? _toggleCcNos;
    
    [XmlAttribute] public int ModWheelReplacementCcNo { get; set; }

    [XmlArray(nameof(ContinuousCcNoRanges))]
    [XmlArrayItem("ContinuousCcNoRange")]
    public List<IntegerRange> ContinuousCcNoRanges { get; set; } = [];

    [XmlArray(nameof(ToggleCcNoRanges))]
    [XmlArrayItem("ToggleCcNoRange")]
    public List<IntegerRange> ToggleCcNoRanges { get; set; } = [];

    internal ImmutableList<int> ContinuousCcNos =>
      _continuousCcNos ??= CreateCcNoList(ContinuousCcNoRanges);

    internal ImmutableList<int> ToggleCcNos =>
      _toggleCcNos ??= CreateCcNoList(ToggleCcNoRanges);

    private static ImmutableList<int> CreateCcNoList(List<IntegerRange> ranges) {
      var list = new List<int>();
      foreach (var range in ranges) {
        for (int ccNo = range.Start; ccNo <= range.End; ccNo++) {
          list.Add(ccNo);
        }
      }
      return list.ToImmutableList();
    }
  }

  public class ProgramCategory {
    [XmlAttribute] public string SoundBank { get; set; } = string.Empty;
    [XmlAttribute] public string Category { get; set; } = string.Empty;
  }

  public class Template {
    [XmlAttribute] public string Path { get; set; } = string.Empty;
  }
}