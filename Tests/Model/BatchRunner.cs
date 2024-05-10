﻿using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A utility that allows developers to run batches without using the GUI.
/// </summary>
/// <remarks>
///   To see the console output after running, select the <see cref="Run" /> test in the
///   IDE's Unit Tests pane.
/// </remarks>
[TestFixture]
[Explicit]
[ExcludeFromCodeCoverage]
public class BatchRunner {
  [Test]
  [SuppressMessage("ReSharper", "CommentTypo")]
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public void Run() {
    Global.ApplicationName = "Falcon Programmer";
    var batch = new Batch(new ConsoleBatchLog());
    // batch.RollForward(null);
    // batch.RollForward("Devinity", "Bass");
    // batch.RollForward("Devinity", "Bass", "Bass Interia");
    // batch.RollForward("Devinity", "Bass", "Comber Bass");
    // batch.RollForward("Devinity", "Bass", "Talking Bass");
    // batch.RollForward("Devinity", "Plucks-Leads", "Harmony Plucks");
    // batch.RollForward("Devinity", "Plucks-Leads", "Pluck Sphere");
    // batch.RollForward("Devinity");
    // batch.RollForward("Eternal Funk");
    // batch.RollForward("Ether Fields");
    // batch.QueryMainDahdsr("Factory");
    // batch.RollForward("Factory", "Bass-Sub", "100in1 1.4");
    // batch.RollForward("Factory", "Bass-Sub", "Balarbas 2.0");
    // batch.RollForward("Factory", "Brutal Bass 2.1", "808 Line");
    // batch.RollForward("Factory", "RetroWave 2.5", "BAS Hawkins Lab");
    // batch.ReuseCc1("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.RollForward("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.ReplaceModWheelWithMacro("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.RollForward("Factory", "VCF-20 Synths 2.5", "Bass Story");
    // batch.RollForward("Factory", "Vocal-Formant", "Vocal Growl Bass 1.6");
    // batch.RollForward("Fluidity");
    // batch.RollForward("Fluidity", "Electronic", "Fluid Sweeper");
    // batch.ReplaceModWheelWithMacro("Fluidity", "Electronic", "Fluid Sweeper");
    // batch.RollForward("Fluidity", "Strings", "Guitar Stream");
    // batch.RollForward("Hypnotic Drive");
    // batch.RollForward("Inner Dimensions");
    // batch.RollForward("Modular Noise");
    // batch.RollForward("Modular Noise", "Bass", "Berghain");
    // batch.RollForward("Organic Keys");
    // batch.RollForward("Organic Keys", "Digitalish");
    // batch.RollForward("Organic Pads");
    // batch.RestoreOriginal("Organic Pads");
    // batch.RollForward("Organic Pads", "Light", "Crystal Caves");
    // batch.PrependPathLineToDescription("Organic Pads", "Light", "Crystal Caves");
    // batch.RollForward("Savage");
    // batch.RollForward("Savage", "Pads-Drones", "Pad Chord Ram");
    // batch.RollForward("Spectre");
    // batch.RollForward("Spectre", "Leads", "LD Showteker");
    // batch.RollForward("Spectre", "Polysynth", "PL Cream");
    // batch.RollForward("Titanium");
    // batch.MoveConnectionsBeforeProperties(null);
    // batch.QueryAdsrMacros(null);
    // batch.QueryCountMacros(null);
    // batch.QueryDelayTypes(null);
    // batch.QueryDahdsrModulations(null);
    // batch.QueryReuseCc1NotSupported(null);
    batch.QueryReverbTypes(null);
  }
}