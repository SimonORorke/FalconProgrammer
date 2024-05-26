using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer.ViewModel;

public class MidiForMacrosViewModel : SettingsWriterViewModelBase {
  private CcNoRangeCollection? _continuousCcNoRanges;
  private int _modWheelReplacementCcNo;
  private CcNoRangeCollection? _toggleCcNoRanges;

  public MidiForMacrosViewModel(IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dialogService, dispatcherService) { }

  [ExcludeFromCodeCoverage]
  public static string CcNoRangeAdvice =>
    "For each of Continuous and Toggle CC Number Ranges, " +
    "if the last End = the last Start, the last range will be extended indefinitely.";

  public CcNoRangeCollection ContinuousCcNoRanges => _continuousCcNoRanges
    ??= new CcNoRangeCollection("Continuous",
      DialogService, DispatcherService);

  [ExcludeFromCodeCoverage]
  public static string ModWheelReplacementAdvice =>
    "Must be > 1 to allow ReplaceModWheelWithMacro and ReuseCc1.";

  [Range(0, 127)]
  public int ModWheelReplacementCcNo {
    get => _modWheelReplacementCcNo;
    set => SetProperty(ref _modWheelReplacementCcNo, value, true);
  }

  public override string PageTitle => "MIDI for Macros";

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
    ModWheelReplacementCcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate(Settings.MidiForMacros.ContinuousCcNoRanges);
    ToggleCcNoRanges.Populate(Settings.MidiForMacros.ToggleCcNoRanges);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    // Provided there are no validation errors for either of the two collections of
    // ranges, base.QueryClose will automatically save this setting, if it has
    // changed, as it is a property of this view model.
    Settings.MidiForMacros.ModWheelReplacementCcNo = ModWheelReplacementCcNo;
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
    if (haveRangesChanged) {
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    // Attempt to save settings if changed.
    return await base.QueryClose(isClosingWindow);
  }
}