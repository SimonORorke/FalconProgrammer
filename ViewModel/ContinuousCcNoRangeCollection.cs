using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class ContinuousCcNoRangeCollection(
  Settings settings,
  IDispatcherService dispatcherService)
  : CcNoRangeCollection(settings, dispatcherService) {
  
  protected override IEnumerable<Settings.IntegerRange> GetRangesFromSettings() {
    return Settings.MidiForMacros.ContinuousCcNoRanges;
  }
}