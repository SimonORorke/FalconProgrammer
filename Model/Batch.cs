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

  internal BatchScriptReader BatchScriptReader {
    get => _batchScriptReader ??= new BatchScriptReader();
    // For tests
    set => _batchScriptReader = value;
  }

  private CancellationToken RunCancellationToken { get; set; }
  private protected Category Category { get; private set; } = null!;
  private List<string> EffectTypes { get; set; } = null!;

  internal IFileSystemService FileSystemService {
    get => _fileSystemService ??= Model.FileSystemService.Default;
    // For tests
    set => _fileSystemService = value;
  }

  public IBatchLog Log { get; }
  private protected FalconProgram Program { get; private set; } = null!;

  internal Settings Settings {
    get => _settings ??= SettingsReader.Read();
    private set => _settings = value;
  }

  internal SettingsReader SettingsReader {
    get => _settingsReader ??= new SettingsReader();
    // For tests
    set => _settingsReader = value;
  }

  protected string SoundBankFolderPath { get; private set; } = null!;
  [PublicAPI] protected string SoundBankName => Path.GetFileName(SoundBankFolderPath);
  public ConfigTask Task { get; private set; }
  public int TaskCount { get; private set; }
  public int TaskNo { get; private set; }
  public event EventHandler? ScriptRunEnded;

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
      case ConfigTask.MoveZeroedMacrosToEnd:
        Program.MoveZeroedMacrosToEnd();
        break;
      case ConfigTask.PrependPathLineToDescription:
        Program.PrependPathLineToDescription();
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
      case ConfigTask.ZeroReleaseMacro:
        Program.ZeroReleaseMacro();
        break;
      case ConfigTask.ZeroReverbMacros:
        Program.ZeroReverbMacros();
        break;
    }
    if (Program.HasBeenUpdated) {
      Program.Save();
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
      case ConfigTask.ReplaceModWheelWithMacro:
        if (!Settings.MidiForMacros.HasModWheelReplacementCcNo) {
          Log.WriteLine(
            "ReplaceModWheelWithMacro is not possible because a mod wheel " +
            "replacement CC number greater than 1 has not been specified.");
          return;
        }
        break;
      case ConfigTask.ReuseCc1:
        if (!Settings.MidiForMacros.HasModWheelReplacementCcNo) {
          Log.WriteLine(
            "ReuseCc1 is not possible because a mod wheel replacement CC " +
            "number greater than 1 has not been specified.");
          return;
        }
        break;
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
    if (Task is ConfigTask.ReplaceModWheelWithMacro or ConfigTask.MoveZeroedMacrosToEnd
        && Category.MustUseGuiScriptProcessor) {
      Log.WriteLine(
        $"Cannot {Task} for category " +
        $@"'{SoundBankName}\{categoryName}' " +
        "because the category's Info page GUI must be specified in a script" +
        " processor.");
      return;
    }
    foreach (string programPath in Category.GetPathsOfProgramFilesToEdit()) {
      Program = CreateFalconProgram(programPath);
      ConfigureProgram();
    }
  }

  private void ConfigureProgramsInSoundBank() {
    if (Task is ConfigTask.ReplaceModWheelWithMacro or ConfigTask.MoveZeroedMacrosToEnd
        && Settings.MustUseGuiScriptProcessor(SoundBankName)) {
      Log.WriteLine(
        $"Cannot {Task} for sound bank " +
        $"{SoundBankName} " +
        "because the sound bank's Info page GUI must be specified in a script" +
        " processor.");
      return;
    }
    foreach (string categoryName in FileSystemService.Folder.GetSubfolderNames(
               SoundBankFolderPath)) {
      ConfigureProgramsInCategory(categoryName);
    }
  }

  [ExcludeFromCodeCoverage]
  private protected virtual Category CreateCategory(string categoryName) {
    var result = new Category(SoundBankFolderPath, categoryName, Settings);
    result.Initialise();
    return result;
  }

  [ExcludeFromCodeCoverage]
  private protected virtual FalconProgram CreateFalconProgram(string path) {
    return new FalconProgram(path, Category, this);
  }

  public string GetOriginalProgramsFolderPath() {
    if (string.IsNullOrEmpty(Settings.OriginalProgramsFolder.Path)) {
      throw new ApplicationException(
        "The original programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path on the Locations page.");
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
        "change the settings folder path on the Locations page.");
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

  protected virtual void OnScriptRunEnded() {
    ScriptRunEnded?.Invoke(this, EventArgs.Empty);
  }

  private void PrepareForEffectTypesQuery(string effectType) {
    EffectTypes = [];
    Log.WriteLine($"{effectType} Types:");
  }

  public virtual BatchScript ReadScript(string batchScriptPath) {
    return BatchScriptReader.Read(batchScriptPath);
  }

  public void RunScript(
    BatchScript batchScript, CancellationToken cancellationToken) {
    RunCancellationToken = cancellationToken;
    try {
      batchScript.Validate();
      // Refresh the settings, as they may have changed between runs.
      Settings = SettingsReader.Read();
      string? soundBankName = GetScopeParameter(batchScript.Scope.SoundBank);
      string? categoryName = GetScopeParameter(batchScript.Scope.Category);
      string? programName = GetScopeParameter(batchScript.Scope.Program);
      var configTasks = batchScript.SequenceTasks();
      TaskCount = configTasks.Count;
      TaskNo = 0;
      foreach (var configTask in configTasks) {
        RunTask(configTask, soundBankName, categoryName, programName);
      }
      Log.Prefix = string.Empty;
      Log.WriteLine("The batch run has finished.");
    } catch (Exception exception) {
      Log.Prefix = string.Empty;
      Log.WriteLine("==========================================");
      if (exception is OperationCanceledException) {
        Log.WriteLine("The batch run has been cancelled.");
      } else {
        try {
          Log.WriteLine(
            $"While running configuration task {Task.ToString()} for program " +
            $"'{Program.Path}', the batch run terminated with this error:");
        } catch (NullReferenceException) {
          Log.WriteLine(
            // Program is null, so it's an initial setup exception, which is unlikely
            // unless you are a developer.
            $"While running configuration task {Task.ToString()}, " + 
            "the batch run terminated with this error:");
        }
        Log.WriteLine("==========================================");
        Log.WriteLine(exception is ApplicationException
          ? exception.Message
          : exception.ToString());
      }
      Log.WriteLine("==========================================");
    }
    OnScriptRunEnded();
    return;

    string? GetScopeParameter(string level) {
      return level is "All" or "" ? null : level;
    }
  }

  public virtual void RunScript(
    string batchScriptPath, CancellationToken cancellationToken) {
    var batchScript = ReadScript(batchScriptPath);
    RunScript(batchScript, cancellationToken);
  }

  /// <summary>
  ///   Runs the specified configuration task for the specified Falcon program(s).
  /// </summary>
  /// <param name="task">
  ///   The configuration task to run.
  /// </param>
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
  internal void RunTask(ConfigTask task,
    string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = task;
    TaskNo++;
    Log.Prefix = $"{task} - ";
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  private void UpdateEffectTypes(IEnumerable<string> effectTypes) {
    foreach (string effectType in effectTypes.Where(effectType =>
               !EffectTypes.Contains(effectType))) {
      EffectTypes.Add(effectType);
    }
  }

  private void WriteEffectTypesQueryResult() {
    foreach (string effectType in EffectTypes) {
      Log.WriteLine(effectType);
    }
  }
}