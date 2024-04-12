using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class ToggleCcNoRangeCollection(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : CcNoRangeCollection("Toggle", dialogService, dispatcherService) {
  protected override List<Settings.IntegerRange> GetRangesFromSettings() {
    return Settings.MidiForMacros.ToggleCcNoRanges;
  }
}