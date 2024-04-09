using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[TestFixture]
public class CcNoRangeCollectionTests : ViewModelTestsBase {
  private CcNoRangeCollection CreateRanges<T>() where T : CcNoRangeCollection {
    return (T)Activator.CreateInstance(typeof(T), [MockDispatcherService])!;
  }
}