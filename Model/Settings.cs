using System.Collections.Immutable;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

/// <summary>
///   To read the settings, use <see cref="SettingsReader" />.
/// </summary>
[XmlRoot(nameof(Settings))]
public class Settings : SerialisationBase {
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

  internal static string GetSettingsPath(string settingsFolderPath) {
    return Path.Combine(settingsFolderPath, "Settings.xml");
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

  public void Write(string? settingsFolderPath = null) {
    if (settingsFolderPath != null) {
      SettingsPath = GetSettingsPath(settingsFolderPath);
    }
    Serialiser.Serialise(typeof(Settings), this, SettingsPath);
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