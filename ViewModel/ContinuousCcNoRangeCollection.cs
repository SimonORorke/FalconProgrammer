using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class ContinuousCcNoRangeCollection(
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : CcNoRangeCollection("Continuous", dialogService, dispatcherService) {
  protected override List<Settings.IntegerRange> GetRangesFromSettings() {
    return Settings.MidiForMacros.ContinuousCcNoRanges;
  }
}