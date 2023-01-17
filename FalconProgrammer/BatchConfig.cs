using JetBrains.Annotations;

namespace FalconProgrammer;

public class BatchConfig {
  public const string ProgramExtension = ".uvip";
  private const string SynthName = "UVI Falcon";
  private Category Category { get; set; } = null!;

  /// <summary>
  ///   Gets or sets the order in which MIDI CC numbers are to be mapped by
  ///   <see cref="UpdateMacroCcs" /> to macros relative to their locations on the Info
  ///   page.
  /// </summary>
  [PublicAPI]
  public LocationOrder MacroCcLocationOrder { get; set; } =
    LocationOrder.TopToBottomLeftToRight;

  /// <summary>
  ///   Gets or sets the maximum number of continuous macros that can already exist in
  ///   a program for <see cref="ReplaceModWheelWithMacro" /> to add a new continuous
  ///   macro to implement the modulations that were previously assigned to the mod
  ///   wheel.
  /// </summary>
  [PublicAPI]
  public int MaxExistingContinuousMacroCount { get; set; } = 3;

  /// <summary>
  ///   Gets or sets the MIDI CC number to be mapped to a program's new mod wheel
  ///   replacement macro by <see cref="ReplaceModWheelWithMacro" />.
  /// </summary>
  [PublicAPI]
  public int ModWheelReplacementCcNo { get; set; } = 34;

  private int NewCcNo { get; set; }
  private int OldCcNo { get; set; }
  private FalconProgram Program { get; set; } = null!;
  private Settings Settings { get; set; } = null!;
  private DirectoryInfo SoundBankFolder { get; set; } = null!;
  private ConfigTask Task { get; set; }

  /// <summary>
  ///   For programs with a Delay macro, changes the Delay macro's value to zero.
  /// </summary>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void ChangeDelayToZero(
    string? soundBankName, string? categoryName = null) {
    Task = ConfigTask.ChangeDelayToZero;
    ConfigurePrograms(soundBankName, categoryName);
  }

  /// <summary>
  ///   Changes every occurrence of the specified old macro MIDI CC number to the specified
  ///   new CC number.
  /// </summary>
  /// <param name="oldCcNo">MIDI CC number to be replaced.</param>
  /// <param name="newCcNo">Replacement MIDI CC number.</param>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void ChangeMacroCcNo(
    int oldCcNo, int newCcNo,
    string? soundBankName, string? categoryName = null) {
    OldCcNo = oldCcNo;
    NewCcNo = newCcNo;
    Task = ConfigTask.ChangeMacroCcNo;
    ConfigurePrograms(soundBankName, categoryName);
  }

  /// <summary>
  ///   For programs with a Reverb macro, changes the Reverb macro's value to zero.
  /// </summary>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void ChangeReverbToZero(
    string? soundBankName, string? categoryName = null) {
    Task = ConfigTask.ChangeReverbToZero;
    ConfigurePrograms(soundBankName, categoryName);
  }

  /// <summary>
  ///   Configures the specified programs as per the required task.
  /// </summary>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  private void ConfigurePrograms(
    string? soundBankName, string? categoryName = null) {
    Settings = Settings.Read();
    if (soundBankName != null) {
      SoundBankFolder = GetSoundBankFolder(soundBankName);
      if (categoryName != null) {
        ConfigureProgramsInCategory(categoryName);
      } else {
        foreach (var categoryFolder in SoundBankFolder.GetDirectories()) {
          ConfigureProgramsInCategory(categoryFolder.Name);
        }
      }
    } else { // All sound banks
      foreach (var soundBankFolder in GetProgramsFolder().GetDirectories()) {
        SoundBankFolder = soundBankFolder;
        foreach (var categoryFolder in SoundBankFolder.GetDirectories()) {
          ConfigureProgramsInCategory(categoryFolder.Name);
        }
      }
    }
  }

  private void ConfigureProgramsInCategory(
    string categoryName) {
    Console.WriteLine("==========================");
    Console.WriteLine($"Category: {categoryName}");
    Category = new Category(categoryName, SoundBankFolder);
    Category.Initialise();
    if (Task == ConfigTask.ReplaceModWheelWithMacro &&
        Category.IsInfoPageLayoutInScript) {
      Console.WriteLine(
        $"Cannot replace mod wheel modulations for category '{categoryName}' " +
        "because the category's Info page layout is defined in a script.");
      return;
    }
    foreach (var programFileToEdit in Category.GetProgramFilesToEdit()) {
      Program = new FalconProgram(programFileToEdit.FullName, Category);
      Program.Read();
      switch (Task) {
        case ConfigTask.ChangeDelayToZero:
          Program.ChangeDelayToZero();
          break;
        case ConfigTask.ChangeMacroCcNo:
          Program.ChangeMacroCcNo(OldCcNo, NewCcNo);
          break;
        case ConfigTask.ChangeReverbToZero:
          Program.ChangeReverbToZero();
          break;
        case ConfigTask.CountMacros:
          Program.CountMacros();
          break;
        case ConfigTask.ReplaceModWheelWithMacro:
          Program.ReplaceModWheelWithMacro(
            ModWheelReplacementCcNo, MaxExistingContinuousMacroCount);
          break;
        case ConfigTask.UpdateMacroCcs:
          Program.UpdateMacroCcs(MacroCcLocationOrder);
          break;
      }
      if (Task != ConfigTask.CountMacros) {
        Program.Save();
      }
    }
  }

  /// <summary>
  ///   For each of the specified Falcon program presets, reports the number of macros.
  /// </summary>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void CountMacros(string? soundBankName, string? categoryName = null) {
    Task = ConfigTask.CountMacros;
    ConfigurePrograms(soundBankName, categoryName);
  }

  private static DirectoryInfo GetProgramsFolder() {
    var synthSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", SynthName));
    if (!synthSoftwareFolder.Exists) {
      throw new ApplicationException(
        $"Cannot find folder '{synthSoftwareFolder.FullName}'.");
    }
    var result = new DirectoryInfo(
      Path.Combine(
        synthSoftwareFolder.FullName, "Programs"));
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
    }
    return result;
  }

  public static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var programsFolder = GetProgramsFolder();
    var result = new DirectoryInfo(
      Path.Combine(
        programsFolder.FullName, soundBankName));
    if (!result.Exists) {
      throw new ApplicationException($"Cannot find folder '{result.FullName}'.");
    }
    return result;
  }

  /// <summary>
  ///   In each of the specified Falcon program presets where it is feasible, replaces
  ///   use of the modulation wheel with a Wheel macro that executes the same
  ///   modulations.
  /// </summary>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void ReplaceModWheelWithMacro(
    string? soundBankName, string? categoryName = null) {
    Task = ConfigTask.ReplaceModWheelWithMacro;
    ConfigurePrograms(soundBankName, categoryName);
  }

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  /// <param name="soundBankName">Null for all sound banks.</param>
  /// <param name="categoryName">
  ///   If <paramref name="soundBankName"/> is specified, null (the default) for all
  ///   categories in the specified sound bank. Otherwise ignored.
  /// </param>
  [PublicAPI]
  public void UpdateMacroCcs(
    string? soundBankName, string? categoryName = null) {
    Task = ConfigTask.UpdateMacroCcs;
    ConfigurePrograms(soundBankName, categoryName);
  }

  private enum ConfigTask {
    ChangeDelayToZero,
    ChangeMacroCcNo,
    ChangeReverbToZero,
    CountMacros,
    ReplaceModWheelWithMacro,
    UpdateMacroCcs
  }
}