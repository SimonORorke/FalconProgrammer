using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using FalconProgrammer.Model.Mpe;

namespace FalconProgrammer.Model.Options;

/// <summary>
///   To read the settings, use <see cref="SettingsReader" />.
/// </summary>
[XmlRoot(nameof(Settings))]
public class Settings : SerialisationBase {
  [XmlElement] public Folder ProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder OriginalProgramsFolder { get; set; } = new Folder();
  [XmlElement] public Folder TemplateProgramsFolder { get; set; } = new Folder();
  [XmlElement] public string ColourScheme { get; set; } = string.Empty;
  [XmlElement] public WindowLocationSettings? WindowLocation { get; set; }

  [XmlElement("MPE")] public MpeSettings Mpe { get; set; } = new MpeSettings();

  [XmlArray("MustUseGuiScriptProcessor")]
  [XmlArrayItem("SoundBankCategory")]
  public List<SoundBankCategorySetting> MustUseGuiScriptProcessorCategories {
    get;
    [ExcludeFromCodeCoverage] set;
  } = [];

  [XmlElement] public MidiForMacros MidiForMacros { get; set; } = new MidiForMacros();

  [XmlArray(nameof(Backgrounds))]
  [XmlArrayItem("Background")]
  public List<BackgroundSetting> Backgrounds { get; [ExcludeFromCodeCoverage] set; } = [];

  [XmlArray(nameof(DoNotZeroReverb))]
  [XmlArrayItem(nameof(ProgramPath))]
  public List<ProgramPath> DoNotZeroReverb { get; [ExcludeFromCodeCoverage] set; } =
    [];

  [XmlElement]
  public SoundBankSpecificSettings SoundBankSpecific { get; set; } =
    new SoundBankSpecificSettings();

  [XmlElement] public BatchSettings Batch { get; set; } = new BatchSettings();

  [XmlIgnore]
  public ColourSchemeId ColourSchemeId =>
    Global.GetEnumValue<ColourSchemeId>(ColourScheme);

  [XmlIgnore] public string SettingsPath { get; set; } = string.Empty;

  /// <summary>
  ///   Gets whether the initial value of a program's Reverb macro may be changed to zero.
  /// </summary>
  /// <remarks>
  ///   Some programs are silent without reverb, in which case setting the initial reverb
  ///   amount to zero should be disallowed by including the program in the
  ///   <see cref="DoNotZeroReverb" /> list in settings.
  /// </remarks>
  internal bool CanChangeReverbToZero(
    string soundBankName, string categoryName, string programName) {
    return !(
      from programPath in DoNotZeroReverb
      where programPath.SoundBank == soundBankName &&
            programPath.Category == categoryName &&
            programPath.Program == programName
      select programPath).Any();
  }

  internal static string GetSettingsPath(string settingsFolderPath) {
    return !string.IsNullOrWhiteSpace(settingsFolderPath)
      ? Path.Combine(settingsFolderPath, "Settings.xml")
      : string.Empty;
  }

  internal bool MustUseGuiScriptProcessor(
    string soundBankName, string? categoryName = null) {
    bool result = categoryName != null && (
      from soundBankCategory in MustUseGuiScriptProcessorCategories
      where soundBankCategory.SoundBank == soundBankName &&
            soundBankCategory.Category == categoryName
      select soundBankCategory).Any();
    if (!result) {
      result = (
        from soundBankCategory in MustUseGuiScriptProcessorCategories
        where soundBankCategory.SoundBank == soundBankName &&
              soundBankCategory.Category == string.Empty
        select soundBankCategory).Any();
    }
    return result;
  }

  internal bool TryGetSoundBankBackgroundImagePath(
    string soundBankName, out string path) {
    path = (
      from background in Backgrounds
      where background.SoundBank == soundBankName
      select background.Path).FirstOrDefault() ?? string.Empty;
    return path != string.Empty;
  }

  public void Write(string? settingsFolderPath = null) {
    if (settingsFolderPath != null) {
      SettingsPath = GetSettingsPath(settingsFolderPath);
    }
    Serialiser.Serialise(this, SettingsPath);
  }
}