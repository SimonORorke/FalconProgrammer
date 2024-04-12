using System.ComponentModel.DataAnnotations;

namespace FalconProgrammer.ViewModel;

public class MidiForMacrosViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  private ContinuousCcNoRangeCollection? _continuousCcNoRanges;
  private int _modWheelReplacementCcNo;
  private ToggleCcNoRangeCollection? _toggleCcNoRanges;

  public ContinuousCcNoRangeCollection ContinuousCcNoRanges => _continuousCcNoRanges
    ??= new ContinuousCcNoRangeCollection(DialogService, DispatcherService);

  [Range(0, 127)]
  public int ModWheelReplacementCcNo {
    get => _modWheelReplacementCcNo;
    set => SetProperty(ref _modWheelReplacementCcNo, value, true);
  }

  public override string PageTitle => "MIDI for Macros";

  public ToggleCcNoRangeCollection ToggleCcNoRanges => _toggleCcNoRanges
    ??= new ToggleCcNoRangeCollection(DialogService, DispatcherService);

  private static void InterpretClosingUpdateResult(ClosingValidationResult updateResult,
    ref bool haveRangesChanged, ref bool canClosePage) {
    if (updateResult.Success) {
      haveRangesChanged = true;
    } else if (!updateResult.CanClosePage) {
      canClosePage = false;
    }
  }

  internal override void Open() {
    base.Open();
    ModWheelReplacementCcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate(Settings);
    ToggleCcNoRanges.Populate(Settings);
  }

  internal override async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    // Provided there are no validation errors for either of the two collections of
    // ranges, base.QueryCloseAsync will automatically save this setting, if it has
    // changed, as it is a property of this view model.
    Settings.MidiForMacros.ModWheelReplacementCcNo = ModWheelReplacementCcNo;
    bool canClosePage = true;
    bool haveRangesChanged = false;
    if (ContinuousCcNoRanges.HasBeenChanged) {
      InterpretClosingUpdateResult(
        await ContinuousCcNoRanges.UpdateSettingsAsync(isClosingWindow), 
        ref haveRangesChanged, ref canClosePage);
      if (!canClosePage) {
        return false;
      }
    }
    if (ToggleCcNoRanges.HasBeenChanged) {
      InterpretClosingUpdateResult(
        await ToggleCcNoRanges.UpdateSettingsAsync(isClosingWindow), 
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
    return await base.QueryCloseAsync(isClosingWindow);
  }
}