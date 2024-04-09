using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class ToggleCcNoRangeCollection(
  Settings settings,
  IDispatcherService dispatcherService)
  : CcNoRangeCollection(settings, dispatcherService) {
  
  protected override IEnumerable<Settings.IntegerRange> GetRangesFromSettings() {
    return Settings.MidiForMacros.ToggleCcNoRanges;
  }
}