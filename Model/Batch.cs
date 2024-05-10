using System.Diagnostics.CodeAnalysis;
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
  
  // public Batch(IBatchLog log, Func<Task> onScriptRunEnded) {
  //   Log = log;
  //   OnScriptRunEnded = onScriptRunEnded;
  // }

  internal BatchScriptReader BatchScriptReader {
    get => _batchScriptReader ??= new BatchScriptReader();
    // For tests
    set => _batchScriptReader = value;
  }

  private CancellationToken RunCancellationToken { get; set; }
  protected Category Category { get; private set; } = null!;
  private List<string> EffectTypes { get; set; } = null!;

  internal IFileSystemService FileSystemService {
    get => _fileSystemService ??= Model.FileSystemService.Default;
    // For tests
    set => _fileSystemService = value;
  }

  public IBatchLog Log { get; }
  public Func<Task>? OnScriptRunEnded { get; set; }
  protected FalconProgram Program { get; private set; } = null!;
  internal Settings Settings => _settings ??= SettingsReader.Read();

  internal SettingsReader SettingsReader {
    get => _settingsReader ??= new SettingsReader();
    // For tests
    set => _settingsReader = value;
  }

  protected string SoundBankFolderPath { get; private set; } = null!;
  [PublicAPI] protected string SoundBankName => Path.GetFileName(SoundBankFolderPath);
  protected ConfigTask Task { get; private set; }
  
  protected virtual void ConfigureProgram() {
    if (RunCancellationToken.IsCancellationRequested) {
      RunCancellationToken.ThrowIfCancellationRequested();
    }
    if (Task != ConfigTask.RestoreOriginal) {
      Program.Read();
    }
    switch (Task) {
      case ConfigTask.InitialiseLayout:
        Program.InitialiseLayout();
        break;
      case ConfigTask.InitialiseValuesAndMoveMacros:
        Program.InitialiseValuesAndMoveMacros();
        break;
      case ConfigTask.QueryAdsrMacros:
        Program.QueryAdsrMacros();
        break;
      case ConfigTask.QueryCountMacros:
        Program.QueryCountMacros();
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
    switch (Task) {
      case ConfigTask.QueryDelayTypes:
        PrepareForEffectTypesQuery("Delay");
        break;
      case ConfigTask.QueryReverbTypes:
        PrepareForEffectTypesQuery("Reverb");
        break;
    }
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
        ConfigureProgramsInSoundBank();
      }
    } else { // All sound banks
      string programsFolderPath = GetProgramsFolderPath();
      foreach (
        string soundBankName1 in FileSystemService.Folder.GetSubfolderNames(
            programsFolderPath)
          .Where(soundBankName1 => soundBankName1 != ".git")) {
        SoundBankFolderPath = Path.Combine(programsFolderPath, soundBankName1);
        ConfigureProgramsInSoundBank();
      }
    }
    if (Task is ConfigTask.QueryDelayTypes or ConfigTask.QueryReverbTypes) {
      WriteEffectTypesQueryResult();
    }
  }

  private void ConfigureProgramsInCategory(
    string categoryName) {
    Category = CreateCategory(categoryName);
    if (Task is ConfigTask.ReplaceModWheelWithMacro
        && Category.MustUseGuiScriptProcessor) {
      Log.WriteLine(
        $"Cannot {Task} for category " +
        $@"'{SoundBankName}\{categoryName}' " +
        "because the category's GUI has to be defined in a script.");
      return;
    }
    foreach (string programPath in Category.GetPathsOfProgramFilesToEdit()) {
      Program = CreateFalconProgram(programPath);
      ConfigureProgram();
    }
  }

  private void ConfigureProgramsInSoundBank() {
    if (Task is ConfigTask.ReplaceModWheelWithMacro
        && Settings.MustUseGuiScriptProcessor(SoundBankName)) {
      Log.WriteLine(
        $"Cannot {Task} for sound bank " +
        $"'{SoundBankName}' " +
        "because the sound bank's GUI has to be defined in a script.");
      return;
    }
    foreach (string categoryName in FileSystemService.Folder.GetSubfolderNames(
               SoundBankFolderPath)) {
      ConfigureProgramsInCategory(categoryName);
    }
  }

  [ExcludeFromCodeCoverage]
  protected virtual Category CreateCategory(string categoryName) {
    var result = new Category(SoundBankFolderPath, categoryName, Settings);
    result.Initialise();
    return result;
  }

  [ExcludeFromCodeCoverage]
  protected virtual FalconProgram CreateFalconProgram(string path) {
    return new FalconProgram(path, Category, this);
  }

  public string GetOriginalProgramsFolderPath() {
    if (string.IsNullOrEmpty(Settings.OriginalProgramsFolder.Path)) {
      throw new ApplicationException(
        "The original programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path in " +
        $"'{SettingsFolderLocation.GetSettingsFolderLocationPath}'.");
    }
    if (!FileSystemService.Folder.Exists(Settings.OriginalProgramsFolder.Path)) {
      throw new ApplicationException(
        "Cannot find original programs folder '" +
        $"{Settings.OriginalProgramsFolder.Path}'.");
    }
    return Settings.OriginalProgramsFolder.Path;
  }

  private string GetProgramsFolderPath() {
    if (string.IsNullOrEmpty(Settings.ProgramsFolder.Path)) {
      throw new ApplicationException(
        "The programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path in " +
        $"'{SettingsFolderLocation.GetSettingsFolderLocationPath()}'.");
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

  private void PrepareForEffectTypesQuery(string effectType) {
    EffectTypes = [];
    Log.WriteLine($"{effectType} Types:");
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
  public void QueryCountMacros(string? soundBankName, string? categoryName = null,
    string? programName = null) {
    Task = ConfigTask.QueryCountMacros;
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
    Task = ConfigTask.QueryDelayTypes;
    ConfigurePrograms(soundBankName, categoryName, programName);
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
    Task = ConfigTask.QueryReverbTypes;
    ConfigurePrograms(soundBankName, categoryName, programName);
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
        "ReplaceModWheelWithMacro is not possible because a mod wheel replacement " +
        "CC number greater than 1 has not been specified.");
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

  public void RunScript(
    BatchScript batchScript, CancellationToken cancellationToken) {
    RunCancellationToken = cancellationToken;
    try {
      batchScript.Validate();
      foreach (var batchTask in batchScript.SequenceTasks()) {
        Task = batchTask.ConfigTask;
        ConfigurePrograms(
          GetScopeParameter(batchTask.SoundBank),
          GetScopeParameter(batchTask.Category),
          GetScopeParameter(batchTask.Program));
      }
      Log.WriteLine("The batch run has finished.");
    } catch (OperationCanceledException) {
      Log.WriteLine("==========================================");
      Log.WriteLine("The batch run has been cancelled.");
      Log.WriteLine("==========================================");
    } catch (Exception exception) {
      Log.WriteLine("==========================================");
      Log.WriteLine("The batch run terminated with this error:");
      Log.WriteLine("==========================================");
      Log.WriteLine(exception is ApplicationException
        ? exception.Message
        : exception.ToString());
      Log.WriteLine("==========================================");
    }
    OnScriptRunEnded!();
    return;

    string? GetScopeParameter(string level) {
      return level is "All" or "" ? null : level;
    }
  }

  public virtual void RunScript(
    string batchScriptPath, CancellationToken cancellationToken) {
    var batchScript = BatchScriptReader.Read(batchScriptPath);
    RunScript(batchScript, cancellationToken);
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

  private void WriteEffectTypesQueryResult() {
    foreach (string effectType in EffectTypes) {
      Log.WriteLine(effectType);
    }
  }
}