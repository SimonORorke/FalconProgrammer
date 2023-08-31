using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void Run() {
    var batch = new Batch();
    // batch.RollForward("Devinity", "Bass", "Comber Bass");
    // batch.RollForward("Devinity", "Bass");
    // batch.RollForward("Devinity", "Plucks-Leads", "Pluck Sphere");
    // batch.RollForward("Devinity");
    // batch.RollForward("Factory", "Brutal Bass 2.1", "808 Line");
    // batch.RollForward("Devinity", "Plucks-Leads", "Harmony Plucks");
    // batch.QueryReuseCc1NotSupported(null);
    // batch.ReuseCc1("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.RollForward("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.ReplaceModWheelWithMacro("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.MoveConnectionsBeforeProperties(null);
    batch.RollForward(null);
  }
}