using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class MidiForMacrosViewModel(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : SettingsWriterViewModelBase(dialogService, dispatcherService) {
  private ContinuousCcNoRangeCollection? _continuousCcNoRanges;
  private ToggleCcNoRangeCollection? _toggleCcNoRanges;

  public ContinuousCcNoRangeCollection ContinuousCcNoRanges => _continuousCcNoRanges
    ??= new ContinuousCcNoRangeCollection(Settings, DispatcherService);

  public CcNoViewModel ModWheelReplacement { get; } = new CcNoViewModel(
    "_Mod Wheel Replacement CC No");

  public override string PageTitle => "MIDI for Macros";

  public ToggleCcNoRangeCollection ToggleCcNoRanges => _toggleCcNoRanges
    ??= new ToggleCcNoRangeCollection(Settings, DispatcherService);

  public override void Open() {
    base.Open();
    ModWheelReplacement.CcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    ContinuousCcNoRanges.Populate();
    ToggleCcNoRanges.Populate();
  }

  public override bool QueryClose() {
    bool haveCollectionsBeenChanged = false;
    if (ContinuousCcNoRanges.HasBeenChanged) {
      haveCollectionsBeenChanged = true;
      UpdateSettingsRanges(ContinuousCcNoRanges,
        Settings.MidiForMacros.ContinuousCcNoRanges);
    }
    if (ToggleCcNoRanges.HasBeenChanged) {
      haveCollectionsBeenChanged = true;
      UpdateSettingsRanges(ToggleCcNoRanges,
        Settings.MidiForMacros.ToggleCcNoRanges);
    }
    if (haveCollectionsBeenChanged) {
      // Notify change, so that Settings will be saved.
      OnPropertyChanged();
    }
    return base.QueryClose(); // Saves settings if changed.
  }

  private static void UpdateSettingsRanges(
    CcNoRangeCollection viewModelRanges,
    List<Settings.IntegerRange> settingsRanges) {
    settingsRanges.Clear();
    settingsRanges.AddRange(
      from range in viewModelRanges
      where !range.IsAdditionItem
      select new Settings.IntegerRange {
        Start = range.Start, 
        End = range.End
      });
  }
}