using JetBrains.Annotations;

namespace FalconProgrammer.Model;

public class Batch {
  private BatchScriptReader? _batchScriptReader;
  private IFileSystemService? _fileSystemService;
  private Settings? _settings;
  private SettingsReader? _settingsReader;

  public Batch(IBatchLog log) {
    Log = log;
  }

  internal BatchScriptReader BatchScriptReader {
    get => _batchScriptReader ??= new BatchScriptReader();
    // For tests
    set => _batchScriptReader = value;
  }

  private Category Category { get; set; } = null!;
  private List<string> EffectTypes { get; set; } = null!;

  internal IFileSystemService FileSystemService {
    get => _fileSystemService ??= Model.FileSystemService.Default;
    // For tests
    set => _fileSystemService = value;
  }

  public IBatchLog Log { get; }
  private int NewCcNo { get; set; }
  private int OldCcNo { get; set; }
  protected FalconProgram Program { get; private set; } = null!;

  internal Settings Settings => _settings ??= SettingsReader.Read();

  internal SettingsReader SettingsReader {
    get => _settingsReader ??= new SettingsReader();
    // For tests
    set => _settingsReader = value;
  }

  protected string SoundBankFolderPath { get; private set; } = null!;
  private string SoundBankName => Path.GetFileName(SoundBankFolderPath);
  protected ConfigTask Task { get; private set; }

  /// <summary>
  ///   Changes every occurrence of the specified old macro MIDI CC number to the
  ///   specified new CC number.
  /// </summary>
  /// <param name="oldCcNo">MIDI CC number to be replaced.</param>
  /// <param name="newCcNo">Replacement MIDI CC number.</param>
  /// <param name="soundBankName">
  ///   The name of the sound bank folder. Null for all sound banks.
  /// </param>
  /// <param name="categoryName">
  ///   The name of the category folder.
  ///   If <paramref name="soundBankName" /> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  /// <param name="programName">
  ///   The program name, excluding the ".uvip" suffix.
  ///   If <paramref name="soundBankName" /> and <paramref name="soundBankName" /> are
  ///   specified, null (the default) for all files in the specified category.
  ///   Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void ChangeMacroCcNo(
    int oldCcNo, int newCcNo,
    string? soundBankName, string? categoryName = null, string? programName = null) {
    OldCcNo = oldCcNo;
    NewCcNo = newCcNo;
    Task = ConfigTask.ChangeMacroCcNo;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  /// <summary>
  ///   Configures the specified programs as per the required task.
  /// </summary>
  /// <param name="soundBankName">
  ///   The name of the sound bank folder. Null for all sound banks.
  /// </param>
  /// <param name="categoryName">
  ///   The name of the category folder.
  ///   If <paramref name="soundBankName" /> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  /// <param name="programName">
  ///   The program name, excluding the ".uvip" suffix.
  ///   If <paramref name="soundBankName" /> and <paramref name="soundBankName" /> are
  ///   specified, null (the default) for all files in the specified category.
  ///   Otherwise ignored.
  /// </param>
  private void ConfigurePrograms(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    if (!string.IsNullOrEmpty(soundBankName)) {
      SoundBankFolderPath = GetSoundBankFolderPath(soundBankName);
      if (!string.IsNullOrEmpty(categoryName)) {
        if (!string.IsNullOrEmpty(programName)) {
          Category = CreateCategory(categoryName);
          string programPath = Category.GetProgramPath(programName);
          Program = CreateFalconProgram(programPath);
          ConfigureProgram();
        } else {
          ConfigureProgramsInCategory(categoryName);
        }
      } else {
        foreach (string categoryName1 in FileSystemService.Folder.GetSubfolderNames(
                   SoundBankFolderPath)) {
          ConfigureProgramsInCategory(categoryName1);
        }
      }
    } else { // All sound banks
      string programsFolderPath = GetProgramsFolderPath();
      foreach (
        string soundBankName1 in FileSystemService.Folder.GetSubfolderNames(
            programsFolderPath)
          .Where(soundBankName1 => soundBankName1 != ".git")) {
        SoundBankFolderPath = Path.Combine(programsFolderPath, soundBankName1);
        foreach (string categoryName1 in FileSystemService.Folder.GetSubfolderNames(
                   SoundBankFolderPath)) {
          ConfigureProgramsInCategory(categoryName1);
        }
      }
    }
  }

  protected virtual void ConfigureProgram() {
    if (Task != ConfigTask.RestoreOriginal) {
      Program.Read();
    }
    switch (Task) {
      case ConfigTask.ChangeMacroCcNo:
        Program.ChangeMacroCcNo(OldCcNo, NewCcNo);
        break;
      case ConfigTask.CountMacros:
        Program.CountMacros();
        break;
      case ConfigTask.InitialiseLayout:
        Program.InitialiseLayout();
        break;
      case ConfigTask.InitialiseValuesAndMoveMacros:
        Program.InitialiseValuesAndMoveMacros();
        break;
      case ConfigTask.MoveConnectionsBeforeProperties:
        Program.MoveConnectionsBeforeProperties();
        break;
      case ConfigTask.QueryAdsrMacros:
        Program.QueryAdsrMacros();
        break;
      case ConfigTask.QueryDahdsrModulations:
        Program.QueryDahdsrModulations();
        break;
      case ConfigTask.PrependPathLineToDescription:
        Program.PrependPathLineToDescription();
        break;
      case ConfigTask.QueryDelayTypes:
        UpdateEffectTypes(Program.QueryDelayTypes());
        break;
      case ConfigTask.QueryMainDahdsr:
        Program.QueryMainDahdsr();
        break;
      case ConfigTask.QueryReverbTypes:
        UpdateEffectTypes(Program.QueryReverbTypes());
        break;
      case ConfigTask.QueryReuseCc1NotSupported:
        Program.QueryReuseCc1NotSupported();
        break;
      case ConfigTask.RemoveDelayEffectsAndMacros:
        Program.RemoveDelayEffectsAndMacros();
        break;
      case ConfigTask.ReplaceModWheelWithMacro:
        Program.ReplaceModWheelWithMacro();
        break;
      case ConfigTask.RestoreOriginal:
        Program.RestoreOriginal();
        break;
      case ConfigTask.ReuseCc1:
        Program.ReuseCc1();
        break;
      case ConfigTask.UpdateMacroCcs:
        Program.UpdateMacroCcs();
        break;
    }
    if (Program.HasBeenUpdated) {
      Program.Save();
      if (Task == ConfigTask.InitialiseLayout) {
        Program.FixCData();
      }
    }
  }

  private void ConfigureProgramsInCategory(
    string categoryName) {
    Category = CreateCategory(categoryName);
    if (Task is ConfigTask.ReplaceModWheelWithMacro
        && Category.MustUseGuiScriptProcessor) {
      // TODO: Sound bank level MustUseGuiScriptProcessor check.
      Log.WriteLine(
        $"Cannot {Task} for category " +
        $"'{SoundBankName}\\{categoryName}' " +
        "because the category's GUI has to be defined in a script.");
      return;
    }
    foreach (string programPath in Category.GetPathsOfProgramFilesToEdit()) {
      Program = CreateFalconProgram(programPath);
      ConfigureProgram();
    }
  }

  /// <summary>
  ///   For each of the specified Falcon program presets, reports the number of macros.
  /// </summary>
  /// <param name="soundBankName">
  ///   The name of the sound bank folder. Null for all sound banks.
  /// </param>
  /// <param name="categoryName">
  ///   The name of the category folder.
  ///   If <paramref name="soundBankName" /> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  /// <param name="programName">
  ///   The program name, excluding the ".uvip" suffix.
  ///   If <paramref name="soundBankName" /> and <paramref name="soundBankName" /> are
  ///   specified, null (the default) for all files in the specified category.
  ///   Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void CountMacros(string? soundBankName, string? categoryName = null,
    string? programName = null) {
    Task = ConfigTask.CountMacros;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  protected virtual Category CreateCategory(string categoryName) {
    var result = new Category(SoundBankFolderPath, categoryName, Settings);
    result.Initialise();
    return result;
  }

  private FalconProgram CreateFalconProgram(string path) {
    return new FalconProgram(path, Category, this);
  }

  public DirectoryInfo GetOriginalProgramsFolder() {
    if (string.IsNullOrEmpty(Settings.OriginalProgramsFolder.Path)) {
      throw new ApplicationException(
        "The original programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path in " +
        $"'{SettingsFolderLocation.GetSettingsFolderLocationPath}'.");
    }
    var result = new DirectoryInfo(Settings.OriginalProgramsFolder.Path);
    if (!result.Exists) {
      throw new ApplicationException(
        $"Cannot find original programs folder '{result.FullName}'.");
    }
    return result;
  }

  private string GetProgramsFolderPath() {
    if (string.IsNullOrEmpty(Settings.ProgramsFolder.Path)) {
      throw new ApplicationException(
        "The programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path in " +
        $"'{SettingsFolderLocation.GetSettingsFolderLocationPath}'.");
    }
    if (!FileSystemService.Folder.Exists(Settings.ProgramsFolder.Path)) {
      throw new ApplicationException(
        $"Cannot find programs folder '{Settings.ProgramsFolder.Path}'.");
    }
    return Settings.ProgramsFolder.Path;
  }

  private string GetSoundBankFolderPath(string soundBankName) {
    string programsFolderPath = GetProgramsFolderPath();
    string result = Path.Combine(programsFolderPath, soundBankName);
    if (!FileSystemService.Folder.Exists(result)) {
      throw new ApplicationException(
        $"Cannot find sound bank folder '{result}'.");
    }
    return result;
  }

  [PublicAPI]
  public void InitialiseLayout(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.InitialiseLayout;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void InitialiseValuesAndMoveMacros(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.InitialiseValuesAndMoveMacros;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void MoveConnectionsBeforeProperties(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.MoveConnectionsBeforeProperties;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void PrependPathLineToDescription(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.PrependPathLineToDescription;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void QueryAdsrMacros(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.QueryAdsrMacros;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void QueryDahdsrModulations(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.QueryDahdsrModulations;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void QueryDelayTypes(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    EffectTypes = [];
    Task = ConfigTask.QueryDelayTypes;
    Log.WriteLine("Delay Types:");
    ConfigurePrograms(soundBankName, categoryName, programName);
    foreach (string effectType in EffectTypes) {
      Log.WriteLine(effectType);
    }
  }

  [PublicAPI]
  public void QueryMainDahdsr(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.QueryMainDahdsr;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void QueryReverbTypes(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    EffectTypes = [];
    Task = ConfigTask.QueryReverbTypes;
    Log.WriteLine("Reverb Types:");
    ConfigurePrograms(soundBankName, categoryName, programName);
    foreach (string effectType in EffectTypes) {
      Log.WriteLine(effectType);
    }
  }

  [PublicAPI]
  public void QueryReuseCc1NotSupported(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.QueryReuseCc1NotSupported;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void RemoveDelayEffectsAndMacros(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.RemoveDelayEffectsAndMacros;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  /// <summary>
  ///   In each of the specified Falcon program presets where it is feasible, replaces
  ///   use of the modulation wheel with a Wheel macro that executes the same
  ///   modulations.
  /// </summary>
  /// <param name="soundBankName">
  ///   The name of the sound bank folder. Null for all sound banks.
  /// </param>
  /// <param name="categoryName">
  ///   The name of the category folder.
  ///   If <paramref name="soundBankName" /> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  /// <param name="programName">
  ///   The program name, excluding the ".uvip" suffix.
  ///   If <paramref name="soundBankName" /> and <paramref name="soundBankName" /> are
  ///   specified, null (the default) for all files in the specified category.
  ///   Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void ReplaceModWheelWithMacro(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    if (!Settings.MidiForMacros.HasModWheelReplacementCcNo) {
      Log.WriteLine(
        "ReplaceModWheelWithMacro is not possible because a mod wheel replacement CC " +
        "number greater than 1 has not been specified.");
      return;
    }
    Task = ConfigTask.ReplaceModWheelWithMacro;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void RestoreOriginal(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.RestoreOriginal;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void ReuseCc1(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    if (!Settings.MidiForMacros.HasModWheelReplacementCcNo) {
      Log.WriteLine(
        "ReuseCc1 is not possible because a mod wheel replacement CC " +
        "number greater than 1 has has not been specified.");
      return;
    }
    Task = ConfigTask.ReuseCc1;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void RollForward(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    RestoreOriginal(soundBankName, categoryName, programName);
    PrependPathLineToDescription(soundBankName, categoryName, programName);
    InitialiseLayout(soundBankName, categoryName, programName);
    UpdateMacroCcs(soundBankName, categoryName, programName);
    RemoveDelayEffectsAndMacros(soundBankName, categoryName, programName);
    InitialiseValuesAndMoveMacros(soundBankName, categoryName, programName);
    ReplaceModWheelWithMacro(soundBankName, categoryName, programName);
    ReuseCc1(soundBankName, categoryName, programName);
  }

  public void RunScript(string batchScriptPath) {
    var batchScript = BatchScriptReader.Read(batchScriptPath);
    batchScript.Validate();
    foreach (var batchTask in batchScript.SequenceTasks()) {
      Task = batchTask.ConfigTask;
      ConfigurePrograms(
        GetScopeParameter(batchTask.SoundBank),
        GetScopeParameter(batchTask.Category),
        GetScopeParameter(batchTask.Program));
    }
    return;

    string? GetScopeParameter(string level) {
      return level is "All" or "" ? null : level;
    }
  }

  private void UpdateEffectTypes(IEnumerable<string> effectTypes) {
    foreach (string effectType in effectTypes.Where(effectType =>
               !EffectTypes.Contains(effectType))) {
      EffectTypes.Add(effectType);
    }
  }

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  /// <param name="soundBankName">
  ///   The name of the sound bank folder. Null for all sound banks.
  /// </param>
  /// <param name="categoryName">
  ///   The name of the category folder.
  ///   If <paramref name="soundBankName" /> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  /// <param name="programName">
  ///   The program name, excluding the ".uvip" suffix.
  ///   If <paramref name="soundBankName" /> and <paramref name="soundBankName" /> are
  ///   specified, null (the default) for all files in the specified category.
  ///   Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void UpdateMacroCcs(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.UpdateMacroCcs;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }
}