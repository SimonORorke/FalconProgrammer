using JetBrains.Annotations;

namespace FalconProgrammer;

public class Batch {
  public const string ProgramExtension = ".uvip";
  private Category Category { get; set; } = null!;
  private List<string> EffectTypes { get; set; } = null!;
  private int NewCcNo { get; set; }
  private int OldCcNo { get; set; }
  private FalconProgram Program { get; set; } = null!;
  private Settings Settings { get; set; } = null!;
  private DirectoryInfo SoundBankFolder { get; set; } = null!;
  private ConfigTask Task { get; set; }

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
    Settings = Settings.Read();
    if (soundBankName != null) {
      SoundBankFolder = GetSoundBankFolder(soundBankName);
      if (categoryName != null) {
        if (programName != null) {
          Category = CreateCategory(categoryName);
          string programPath = Category.GetProgramFile(programName).FullName;
          Program = new FalconProgram(programPath, Category);
          ConfigureProgram();
        } else {
          ConfigureProgramsInCategory(categoryName);
        }
      } else {
        foreach (var categoryFolder in SoundBankFolder.GetDirectories()) {
          ConfigureProgramsInCategory(categoryFolder.Name);
        }
      }
    } else { // All sound banks
      foreach (var soundBankFolder in GetProgramsFolder().GetDirectories()
                 .Where(soundBankFolder => soundBankFolder.Name != ".git")) {
        SoundBankFolder = soundBankFolder;
        foreach (var categoryFolder in SoundBankFolder.GetDirectories()) {
          ConfigureProgramsInCategory(categoryFolder.Name);
        }
      }
    }
  }

  private void ConfigureProgram() {
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
    }
  }

  private void ConfigureProgramsInCategory(
    string categoryName) {
    Category = CreateCategory(categoryName);
    if (Task is ConfigTask.ReplaceModWheelWithMacro
        && Category.InfoPageMustUseScript) {
      // Console.WriteLine(
      //   $"Cannot {Task} for category " +
      //   $"'{SoundBankFolder.Name}\\{categoryName}' " +
      //   "because the category's Info page layout has to be defined in a script.");
      return;
    }
    foreach (var programFileToEdit in Category.GetProgramFilesToEdit()) {
      Program = new FalconProgram(programFileToEdit.FullName, Category);
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
  public void CountMacros(string? soundBankName, string? categoryName = null, string? programName = null) {
    Task = ConfigTask.CountMacros;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  private Category CreateCategory(string categoryName) {
    var result = new Category(SoundBankFolder, categoryName, Settings);
    result.Initialise();
    return result;
  }

  private DirectoryInfo GetProgramsFolder() {
    if (string.IsNullOrEmpty(Settings.ProgramsFolder.Path)) {
      throw new ApplicationException(
        "The programs folder is not specified in settings file " +
        $"'{Settings.SettingsPath}'. If that's not the correct settings file, " +
        "change the settings folder path in " +
        $"'{SettingsFolderLocation.GetSettingsFolderLocationFile().FullName}'.");
    }
    var result = new DirectoryInfo(Settings.ProgramsFolder.Path);
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
    }
    return result;
  }

  private DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var programsFolder = GetProgramsFolder();
    var result = new DirectoryInfo(
      Path.Combine(
        programsFolder.FullName, soundBankName));
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
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
    EffectTypes = new List<string>();
    Task = ConfigTask.QueryDelayTypes;
    Console.WriteLine("Delay Types:");
    ConfigurePrograms(soundBankName, categoryName, programName);
    foreach (string effectType in EffectTypes) {
      Console.WriteLine(effectType);
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
    EffectTypes = new List<string>();
    Task = ConfigTask.QueryReverbTypes;
    Console.WriteLine("Reverb Types:");
    ConfigurePrograms(soundBankName, categoryName, programName);
    foreach (string effectType in EffectTypes) {
      Console.WriteLine(effectType);
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
    Task = ConfigTask.ReuseCc1;
    ConfigurePrograms(soundBankName, categoryName, programName);
  }

  [PublicAPI]
  public void RollForward(
    string? soundBankName, string? categoryName = null, string? programName = null) {
    RestoreOriginal(soundBankName, categoryName, programName);
    PrependPathLineToDescription(soundBankName, categoryName, programName);
    InitialiseLayout(soundBankName, categoryName, programName);
    // UpdateMacroCcs(soundBankName, categoryName, programName);
    // RemoveDelayEffectsAndMacros(soundBankName, categoryName, programName);
    // InitialiseValuesAndMoveMacros(soundBankName, categoryName, programName);
    // ReplaceModWheelWithMacro(soundBankName, categoryName, programName);
    // ReuseCc1(soundBankName, categoryName, programName);
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

  private enum ConfigTask {
    ChangeMacroCcNo,
    CountMacros,
    InitialiseLayout,
    InitialiseValuesAndMoveMacros,
    MoveConnectionsBeforeProperties,
    PrependPathLineToDescription,
    QueryAdsrMacros,
    QueryDahdsrModulations,
    QueryDelayTypes,
    QueryMainDahdsr,
    QueryReverbTypes,
    QueryReuseCc1NotSupported,
    RemoveDelayEffectsAndMacros,
    ReplaceModWheelWithMacro,
    RestoreOriginal,
    ReuseCc1,
    UpdateMacroCcs
  }
}