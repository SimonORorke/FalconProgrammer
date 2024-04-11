using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class ContinuousCcNoRangeCollection(
  IDispatcherService dispatcherService)
  : CcNoRangeCollection(dispatcherService) {
  protected override List<Settings.IntegerRange> GetRangesFromSettings() {
    return Settings.MidiForMacros.ContinuousCcNoRanges;
  }
}