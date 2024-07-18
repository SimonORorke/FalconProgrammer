using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FalconProgrammer.ViewModel;

public partial class MidiForMacrosViewModel : SettingsWriterViewModelBase {
  /// <summary>
  ///   Generates <see cref="AppendCcNoToMacroDisplayNames" /> property.
  /// </summary>
  [ObservableProperty] private bool _appendCcNoToMacroDisplayNames;

  private CcNoRangeCollection? _continuousCcNoRanges;
  private DoNotReplaceModWheelCollection? _doNotReplaceModWheelSoundBanks;
  private int? _modWheelReplacementCcNo;
  private CcNoRangeCollection? _toggleCcNoRanges;

  public MidiForMacrosViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public static string AppendCcNoCaption =>
    "_Append the CC number to each macro's display name when updating the CCs " + 
    "for programs that do not have script-defined GUIs.";

  [ExcludeFromCodeCoverage]
  public static string CcNoRangeAdvice =>
    "For each of Continuous and Toggle CC Number Ranges, " +
    "if the last End = the last Start, the last range will be extended indefinitely." +
    Environment.NewLine + 
    "MIDI CC 38 does not work when assigned to a control on a " +
    "script-based Info page. (See manual.)";

  public CcNoRangeCollection ContinuousCcNoRanges => _continuousCcNoRanges
    ??= new CcNoRangeCollection("Continuous",
      DialogService, DispatcherService);

  public DoNotReplaceModWheelCollection DoNotReplaceModWheelSoundBanks => 
    _doNotReplaceModWheelSoundBanks ??= new DoNotReplaceModWheelCollection(
      FileSystemService, DispatcherService);

  [ExcludeFromCodeCoverage]
  public static string ModWheelReplacementAdvice =>
    "Must be > 1 to allow ReplaceModWheelWithMacro and ReuseCc1.";

  [Range(0, 127)]
  public int? ModWheelReplacementCcNo {
    get => _modWheelReplacementCcNo;
    set => SetProperty(ref _modWheelReplacementCcNo, value, true);
  }

  [ExcludeFromCodeCoverage]
  public override string PageTitle => 
    "MIDI CC assignments for macros, for update by the AssignMacroCcs task.";
  
  public override string TabTitle => "MIDI for Macros";

  public CcNoRangeCollection ToggleCcNoRanges => _toggleCcNoRanges
    ??= new CcNoRangeCollection("Toggle",
      DialogService, DispatcherService);

  private void InterpretClosingUpdateResult(ClosingValidationResult updateResult,
    ref bool haveRangesChanged, ref bool canClosePage) {
    if (updateResult.Success) {
      haveRangesChanged = true;
    } else if (!updateResult.CanClosePage) {
      IsFixingError = true;
      canClosePage = false;
    }
  }

  internal override async Task Open() {
    await base.Open();
    var validator = new SettingsValidator(this,
      "Sound banks cannot be updated", TabTitle);
    var soundBanks =
      await validator.GetProgramsFolderSoundBankNames();
    if (soundBanks.Count == 0) {
      return;
    }
    DoNotReplaceModWheelSoundBanks.Populate(Settings, soundBanks);
    AppendCcNoToMacroDisplayNames = Settings.MidiForMacros.AppendCcNoToMacroDisplayNames;
    ModWheelReplacementCcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate(Settings.MidiForMacros.ContinuousCcNoRanges);
    ToggleCcNoRanges.Populate(Settings.MidiForMacros.ToggleCcNoRanges);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    // Provided there are no validation errors for either of the two collections of
    // ranges, base.QueryClose will automatically save these two settings, if changed,
    // as they are properties of this view model.
    Settings.MidiForMacros.AppendCcNoToMacroDisplayNames = AppendCcNoToMacroDisplayNames;
    Settings.MidiForMacros.ModWheelReplacementCcNo = ModWheelReplacementCcNo ?? 0;
    bool canClosePage = true;
    bool haveRangesChanged = false;
    if (ContinuousCcNoRanges.HasBeenChanged) {
      InterpretClosingUpdateResult(
        await ContinuousCcNoRanges.UpdateSettings(isClosingWindow),
        ref haveRangesChanged, ref canClosePage);
      if (!canClosePage) {
        return false;
      }
    }
    if (ToggleCcNoRanges.HasBeenChanged) {
      InterpretClosingUpdateResult(
        await ToggleCcNoRanges.UpdateSettings(isClosingWindow),
        ref haveRangesChanged, ref canClosePage);
      if (!canClosePage) {
        return false;
      }
    }
    DoNotReplaceModWheelSoundBanks.UpdateSettings();
    if (haveRangesChanged || DoNotReplaceModWheelSoundBanks.HasBeenChanged) {
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    // Attempt to save settings if changed.
    return await base.QueryClose(isClosingWindow);
  }
}