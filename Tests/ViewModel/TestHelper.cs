using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

internal static class TestHelper {
  public static CcNoRangeViewModel CreateCcNoRangeAdditionItem(int? start, int? end) {
    return new CcNoRangeViewModel(true) {
      Start = start,
      End = end
    };
  }
}