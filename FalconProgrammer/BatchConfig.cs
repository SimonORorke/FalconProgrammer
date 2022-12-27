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

  private FalconProgram Program { get; set; } = null!;
  private DirectoryInfo SoundBankFolder { get; set; } = null!;
  private ConfigTask Task { get; set; }

  private void ConfigurePrograms(
    string soundBankName, string? categoryName = null) {
    SoundBankFolder = GetSoundBankFolder(soundBankName);
    if (categoryName != null) {
      ConfigureProgramsInCategory(categoryName);
    } else {
      foreach (var folder in SoundBankFolder.GetDirectories()) {
        if (!folder.Name.EndsWith(" ORIGINAL") && !folder.Name.EndsWith(" TEMPLATE")) {
          ConfigureProgramsInCategory(folder.Name);
        }
      }
    }
  }

  private void ConfigureProgramsInCategory(string categoryName) {
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
        case ConfigTask.ReplaceModWheelWithMacro:
          Program.ReplaceModWheelWithMacro(
            ModWheelReplacementCcNo, MaxExistingContinuousMacroCount);
          break;
        case ConfigTask.UpdateMacroCcs:
          Program.UpdateMacroCcs(MacroCcLocationOrder);
          break;
      }
      Program.Save();
    }
  }

  public static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var synthSoftwareFolder = new DirectoryInfo(
      Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Music", "Software", SynthName));
    if (!synthSoftwareFolder.Exists) {
      throw new ApplicationException(
        $"Cannot find sound bank folder '{synthSoftwareFolder.FullName}'.");
    }
    var result = new DirectoryInfo(
      Path.Combine(
        synthSoftwareFolder.FullName, "Programs", soundBankName));
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
  [PublicAPI]
  public void ReplaceModWheelWithMacro(
    string soundBankName, string? categoryName = null) {
    Task = ConfigTask.ReplaceModWheelWithMacro;
    ConfigurePrograms(soundBankName, categoryName);
  }

  /// <summary>
  ///   Configures macro CCs for Falcon program presets.
  /// </summary>
  [PublicAPI]
  public void UpdateMacroCcs(
    string soundBankName, string? categoryName = null) {
    Task = ConfigTask.UpdateMacroCcs;
    ConfigurePrograms(soundBankName, categoryName);
  }

  private enum ConfigTask {
    ReplaceModWheelWithMacro,
    UpdateMacroCcs
  }
}