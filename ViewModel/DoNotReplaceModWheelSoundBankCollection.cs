using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class DoNotReplaceModWheelSoundBankCollection : SoundBankCollection {
  public DoNotReplaceModWheelSoundBankCollection(IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(fileSystemService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public static string AccessibleTitle =>
    "_Do not run ReplaceModWheelWithMacro or ReuseCc1 for these sound banks.";

  protected override List<string> SettingsSoundBanks =>
    Settings.MidiForMacros.DoNotReplaceModWheelWithMacroSoundBanks;
}