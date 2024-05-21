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
    // batch.RollForward("Factory", "Bass-Sub", "100in1 1.4");
    // batch.RollForward("Factory", "Bass-Sub", "Balarbas 2.0");
    // batch.RollForward("Factory", "Brutal Bass 2.1", "808 Line");
    // batch.RollForward("Factory", "RetroWave 2.5", "BAS Hawkins Lab");
    // batch.RollForward("Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.RollForward("Factory", "VCF-20 Synths 2.5", "Bass Story");
    // batch.RollForward("Factory", "Vocal-Formant", "Vocal Growl Bass 1.6");
    // batch.RollForward("Fluidity");
    // batch.RollForward("Fluidity", "Electronic", "Fluid Sweeper");
    // batch.RollForward("Fluidity", "Strings", "Guitar Stream");
    // batch.RollForward("Hypnotic Drive");
    // batch.RollForward("Inner Dimensions");
    // batch.RollForward("Modular Noise");
    // batch.RollForward("Modular Noise", "Bass", "Berghain");
    // batch.RollForward("Organic Keys");
    // batch.RollForward("Organic Keys", "Digitalish");
    // batch.RollForward("Organic Pads");
    // batch.RollForward("Organic Pads", "Light", "Crystal Caves");
    // batch.RollForward("Savage");
    // batch.RollForward("Savage", "Pads-Drones", "Pad Chord Ram");
    // batch.RollForward("Spectre");
    // batch.RollForward("Spectre", "Leads", "LD Showteker");
    // batch.RollForward("Spectre", "Polysynth", "PL Cream");
    // batch.RollForward("Titanium");
    // batch.RunTask(ConfigTask.QueryMainDahdsr, "Factory");
    // batch.RunTask(ConfigTask.ReplaceModWheelWithMacro, "Factory", "RetroWave 2.5", "PAD Midnight Organ");
    // batch.RunTask(ConfigTask.ReplaceModWheelWithMacro, "Fluidity", "Electronic", "Fluid Sweeper");
    // batch.RunTask(ConfigTask.RestoreOriginal, "Organic Pads");
    // batch.RunTask(ConfigTask.PrependPathLineToDescription, "Organic Pads", "Light", "Crystal Caves");
    // batch.RunTask(ConfigTask.QueryAdsrMacros, null);
    // batch.RunTask(ConfigTask.QueryCountMacros, null);
    // batch.RunTask(ConfigTask.QueryDelayTypes, null);
    // batch.RunTask(ConfigTask.QueryDahdsrModulations, null);
    // batch.RunTask(ConfigTask.QueryReuseCc1NotSupported, null);
    batch.RunTask(ConfigTask.QueryReverbTypes, null);
  }
}