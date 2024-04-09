using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class MidiForMacrosViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  private ContinuousCcNoRangeCollection? _continuousCcNoRanges;
  private ToggleCcNoRangeCollection? _toggleCcNoRanges;

  public ContinuousCcNoRangeCollection ContinuousCcNoRanges => _continuousCcNoRanges
    ??= new ContinuousCcNoRangeCollection(DispatcherService);

  public CcNoViewModel ModWheelReplacement { get; } = new CcNoViewModel(
    "_Mod Wheel Replacement CC No");

  public override string PageTitle => "MIDI for Macros";

  public ToggleCcNoRangeCollection ToggleCcNoRanges => _toggleCcNoRanges
    ??= new ToggleCcNoRangeCollection(DispatcherService);

  public override void Open() {
    base.Open();
    ModWheelReplacement.CcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate(Settings);
    ToggleCcNoRanges.Populate(Settings);
  }

  public override bool QueryClose() {
    Settings.MidiForMacros.ModWheelReplacementCcNo = ModWheelReplacement.CcNo; 
    bool haveCollectionsBeenChanged = false;
    if (ContinuousCcNoRanges.HasBeenChanged) {
      haveCollectionsBeenChanged = true;
      ContinuousCcNoRanges.UpdateSettings();
    }
    if (ToggleCcNoRanges.HasBeenChanged) {
      haveCollectionsBeenChanged = true;
      ToggleCcNoRanges.UpdateSettings();
    }
    if (haveCollectionsBeenChanged) {
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return base.QueryClose(); // Saves settings if changed.
  }
}