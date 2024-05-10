using System.Diagnostics.CodeAnalysis;
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
  public async Task Run() {
    Global.ApplicationName = "Falcon Programmer";
    var batch = new Batch(new ConsoleBatchLog());
    // await batch.RollForward(null);
    // await batch.RollForward("Devinity", "Bass");
    // await batch.RollForward("Devinity", "Bass", "Bass Interia");
    // await batch.RollForward("Devinity", "Bass", "Comber Bass");
    // await batch.RollForward("Devinity", "Bass", "Talking Bass");
    // await batch.RollForward("Devinity", "Plucks-Leads", "Harmony Plucks");
    // await batch.RollForward("Devinity", "Plucks-Leads", "Pluck Sphere");
    // await batch.RollForward("Devinity");
    // await batch.RollForward("Eternal Funk");
    // await batch.RollForward("Ether Fields");
    // await batch.QueryMainDahdsr("Factory");
    // await batch.RollForward("Factory", "Bass-Sub", "100in1 1.4");
    // await batch.RollForward("Factory", "Bass-Sub", "Balarbas 2.0");
    // await batch.RollForward("Factory", "Brutal Bass 2.1", "808 Line");
    // await batch.RollForward("Factory", "RetroWave 2.5", "BAS Hawkins Lab");
    // await batch.ReuseCc1("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // await batch.RollForward("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // await batch.ReplaceModWheelWithMacro("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // await batch.RollForward("Factory", "VCF-20 Synths 2.5", "Bass Story");
    // await batch.RollForward("Factory", "Vocal-Formant", "Vocal Growl Bass 1.6");
    // await batch.RollForward("Fluidity");
    // await batch.RollForward("Fluidity", "Electronic", "Fluid Sweeper");
    // await batch.ReplaceModWheelWithMacro("Fluidity", "Electronic", "Fluid Sweeper");
    // await batch.RollForward("Fluidity", "Strings", "Guitar Stream");
    // await batch.RollForward("Hypnotic Drive");
    // await batch.RollForward("Inner Dimensions");
    // await batch.RollForward("Modular Noise");
    // await batch.RollForward("Modular Noise", "Bass", "Berghain");
    // await batch.RollForward("Organic Keys");
    // await batch.RollForward("Organic Keys", "Digitalish");
    // await batch.RollForward("Organic Pads");
    // await batch.RestoreOriginal("Organic Pads");
    // await batch.RollForward("Organic Pads", "Light", "Crystal Caves");
    // await batch.PrependPathLineToDescription("Organic Pads", "Light", "Crystal Caves");
    // await batch.RollForward("Savage");
    // await batch.RollForward("Savage", "Pads-Drones", "Pad Chord Ram");
    // await batch.RollForward("Spectre");
    // await batch.RollForward("Spectre", "Leads", "LD Showteker");
    // await batch.RollForward("Spectre", "Polysynth", "PL Cream");
    // await batch.RollForward("Titanium");
    // await batch.MoveConnectionsBeforeProperties(null);
    // await batch.QueryAdsrMacros(null);
    // await batch.QueryCountMacros(null);
    // await batch.QueryDelayTypes(null);
    // await batch.QueryDahdsrModulations(null);
    // await batch.QueryReuseCc1NotSupported(null);
    await batch.QueryReverbTypes(null);
  }
}