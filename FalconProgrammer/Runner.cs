using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  public static void Run() {
    var batch = new Batch();
    // batch.RollForward("Factory", "Test");
    batch.ReplaceModWheelWithMacro("Factory", "Test");
    // batch.ReplaceModWheelWithMacro(null);
  }
}