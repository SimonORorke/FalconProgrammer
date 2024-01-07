using System.Collections.Immutable;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

[XmlRoot(nameof(Settings))]
public class Settings {
  public const string DefaultSettingsFolderPath =
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  private ISerializer? _serializer;
  [XmlElement] public Folder BatchScriptsFolder { get; set; } = new Folder();
  [XmlElement] public Folder ProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder OriginalProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder TemplateProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Template DefaultTemplate { get; set; } = new Template();

  [XmlArray("MustUseGuiScriptProcessor")]
  [XmlArrayItem(nameof(ProgramCategory))]
  public List<ProgramCategory> MustUseGuiScriptProcessorCategories { get; set; } = [];

  [XmlElement] public MacrosMidi MidiForMacros { get; set; } = new MacrosMidi();

  /// <summary>
  ///   A utility that can serialize an object to a file. The default is a real
  ///   <see cref="Serializer" />. Can be set to a mock serializer for unit testing. 
  /// </summary>
  [XmlIgnore] public ISerializer Serializer {
    get => _serializer ??= Model.Serializer.Default;
    set => _serializer = value;
  }
  
  [XmlIgnore] public string SettingsPath { get; set; } = string.Empty;

  internal static FileInfo GetSettingsFile(string settingsFolderPath) {
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
    IFileSystemService fileSystemService,
    ISerializer writeSerializer,
    string defaultSettingsFolderPath = "",
    string applicationName = SettingsFolderLocation.DefaultApplicationName) {
    var settingsFolderLocation = SettingsFolderLocation.Read(
      fileSystemService, writeSerializer, applicationName);
    if (string.IsNullOrEmpty(settingsFolderLocation.Path)) {
      settingsFolderLocation.Path = defaultSettingsFolderPath;
      settingsFolderLocation.Write();
    }
    var settingsFile = GetSettingsFile(settingsFolderLocation.Path);
    Settings result;
    if (settingsFile.Exists) {
      using var reader = new StreamReader(settingsFile.FullName);
      var readSerializer = new XmlSerializer(typeof(Settings));
      result = (Settings)readSerializer.Deserialize(reader)!;
      result.SettingsPath = settingsFile.FullName;
    } else {
      result = new Settings { SettingsPath = settingsFile.FullName };
    }
    result.Serializer = writeSerializer;
    return result;
  }

  public void Write(string? settingsFolderPath = null) {
    if (settingsFolderPath != null) {
      var settingsFile = GetSettingsFile(settingsFolderPath);
      SettingsPath = settingsFile.FullName;
    }
    Serializer.Serialize(typeof(Settings), this, SettingsPath);
  }

  public class Folder {
    [XmlAttribute] public string Path { get; set; } = string.Empty;
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