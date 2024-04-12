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

  private static void InterpretUpdateResult(InteractiveValidationResult updateResult,
    ref bool haveRangesChanged, ref bool canCloseWindow) {
    if (updateResult.Success) {
      haveRangesChanged = true;
    } else if (!updateResult.CanCloseWindow) {
      canCloseWindow = false;
    }
  }

  internal override void Open() {
    base.Open();
    ModWheelReplacementCcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate(Settings);
    ToggleCcNoRanges.Populate(Settings);
  }

  internal override async Task<bool> QueryCloseAsync(bool isClosingWindow = false) {
    // bass.QueryCloseAsync will automatically save this setting, if it has changed, as
    // it is a property of this view model.
    Settings.MidiForMacros.ModWheelReplacementCcNo = ModWheelReplacementCcNo;
    bool canCloseWindow = true;
    bool haveRangesChanged = false;
    if (ContinuousCcNoRanges.HasBeenChanged) {
      InterpretUpdateResult(
        await ContinuousCcNoRanges.UpdateSettingsAsync(isClosingWindow), 
        ref haveRangesChanged, ref canCloseWindow);
    }
    if (ToggleCcNoRanges.HasBeenChanged) {
      InterpretUpdateResult(
        await ToggleCcNoRanges.UpdateSettingsAsync(isClosingWindow), 
        ref haveRangesChanged, ref canCloseWindow);
    }
    if (haveRangesChanged) {
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    // Attempt to save settings if changed.
    if (!await base.QueryCloseAsync(isClosingWindow)) {
      canCloseWindow = false;
    }
    return canCloseWindow;
  }
}