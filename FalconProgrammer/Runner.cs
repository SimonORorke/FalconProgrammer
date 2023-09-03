﻿using System.Diagnostics.CodeAnalysis;

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
    // batch.RollForward("Fluidity", "Strings", "Guitar Stream");
    // batch.ReplaceModWheelWithMacro("Fluidity", "Electronic", "Fluid Sweeper");
    // batch.RollForward("Fluidity", "Electronic", "Fluid Sweeper");
    // batch.RollForward("Fluidity");
    // batch.RollForward("Ether Fields");
    // batch.RollForward("Spectre");
    // batch.RollForward("Spectre", "Polysynth", "PL Cream");
    // batch.RollForward("Factory", "Bass-Sub", "Balarbas 2.0");
    // batch.RollForward("Spectre", "Leads", "LD Showteker");
    batch.RollForward(null);
  }
}