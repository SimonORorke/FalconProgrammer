using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public static void Run() {
    var batch = new Batch();
    //batch.MoveConnectionsBeforeProperties("Devinity", "Bass", "Comber Bass");
    // batch.MoveConnectionsBeforeProperties(null);
    // batch.RollForward("Devinity", "Bass", "Comber Bass");
    // batch.RollForward("Devinity", "Bass");
    // batch.RollForward("Devinity", "Plucks-Leads", "Pluck Sphere");
    // batch.RollForward("Devinity");
    // batch.RollForward("Factory", "Brutal Bass 2.1", "808 Line");
    // batch.RollForward("Devinity", "Plucks-Leads", "Harmony Plucks");
    batch.RollForward(null);
  }
}