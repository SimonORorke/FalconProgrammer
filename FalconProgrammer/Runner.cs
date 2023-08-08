using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void Run() {
    var batch = new Batch();
    // batch.RollForward(null);
    // batch.UpdateMacroCcs("Factory", "Bass-Sub", "Growl Alarma");
    batch.UpdateMacroCcs(null);
    // batch.ReplaceModWheelWithMacro("Factory", "Test");
    // batch.ReplaceModWheelWithMacro(null);
  }
}