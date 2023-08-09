using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void Run() {
    var batch = new Batch();
    // batch.RollForward(null);
    // batch.UpdateMacroCcs("Factory", "Bass-Sub", "Growl Alarma");
    // batch.UpdateMacroCcs(null);
    // batch.ReplaceModWheelWithMacro(
    //   "Factory", "Bass-Sub", "Growl Alarma");
    // batch.ReplaceModWheelWithMacro("Devinity", "Bass");
    // batch.BypassDelayEffects(null);
    // batch.ChangeReverbToZero(null);
    // batch.RemoveDelayEffectsAndMacros("Devinity", "Bass", "Pogo Bass");
    // batch.RemoveDelayEffectsAndMacros("Devinity", "Bass", "Tumbler Bass");
    // batch.RemoveDelayEffectsAndMacros(null);
    // batch.ChangeReverbToZero(null);
    batch.ReplaceModWheelWithMacro("Eternal Funk", "Synths", "Bell Shaka");
    // batch.ReplaceModWheelWithMacro(null);
  }
}