using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  public static void Run() {
    var batch = new BatchProcessor();
    // batch.ReplaceModWheelWithMacro(null);
    // batch.ReplaceModWheelWithMacro("Eternal Funk", "Test");
    // batch.ReplaceModWheelWithMacro("Titanium", "Test");
  }
}