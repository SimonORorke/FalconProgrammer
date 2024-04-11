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
    ??= new ContinuousCcNoRangeCollection(DispatcherService);

  [Range(0, 127)]
  public int ModWheelReplacementCcNo {
    get => _modWheelReplacementCcNo;
    set => SetProperty(ref _modWheelReplacementCcNo, value, true);
  }

  public override string PageTitle => "MIDI for Macros";

  public ToggleCcNoRangeCollection ToggleCcNoRanges => _toggleCcNoRanges
    ??= new ToggleCcNoRangeCollection(DispatcherService);

  internal override void Open() {
    base.Open();
    ModWheelReplacementCcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate(Settings);
    ToggleCcNoRanges.Populate(Settings);
  }

  internal override async Task<bool> QueryClose(bool isClosingWindow = false) {
    Settings.MidiForMacros.ModWheelReplacementCcNo = ModWheelReplacementCcNo; 
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
    return await base.QueryClose(isClosingWindow); // Saves settings if changed.
  }
}