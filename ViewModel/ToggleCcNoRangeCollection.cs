using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class ToggleCcNoRangeCollection(
  IDispatcherService dispatcherService)
  : CcNoRangeCollection(dispatcherService) {
  protected override List<Settings.IntegerRange> GetRangesFromSettings() {
    return Settings.MidiForMacros.ToggleCcNoRanges;
  }
}