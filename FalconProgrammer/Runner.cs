using System.Diagnostics.CodeAnalysis;

namespace FalconProgrammer;

public static class Runner {
  [SuppressMessage("ReSharper", "CommentTypo")]
  public static void Run() {
    var batch = new BatchProcessor();
    // batch.RestoreOriginal(null);
    // batch.PrependPathLineToDescription(null);
    // batch.UpdateMacroCcs("Factory");
    // batch.UpdateMacroCcs("Factory", "Lo-Fi 2.5");
    // batch.UpdateMacroCcs("Factory", "Pure Additive 2.0");
    // batch.UpdateMacroCcs("Factory", "Test");
    // batch.UpdateMacroCcs("Hypnotic Drive", "Test");
    // batch.UpdateMacroCcs("Hypnotic Drive");
    // batch.UpdateMacroCcs("Inner Dimensions", "Test");
    // batch.UpdateMacroCcs(null);
    // batch.RollForward("Titanium");
    // batch.BypassDelayEffects(null);
    // batch.RollForward("Devinity", "Bass");
    // batch.UpdateMacroCcs("Devinity", "Test");
    // batch.RollForward("Factory", "Test");
    // batch.RollForward(null);
    //
    // batch.QueryDelayTypes(null);
    // batch.QueryReverbTypes(null);
    // batch.ChangeReverbToZero("Hypnotic Drive");
    // batch.BypassDelayEffects(null);
    // batch.PrependPathLineToDescription("Factory", "DelayDisabled");
    // batch.RestoreOriginal("Hypnotic Drive");
    // batch.RestoreOriginal("Titanium");
    // batch.RollForward("Hypnotic Drive");
    // batch.RollForward("Hypnotic Drive", "Test");
    // batch.PrependPathLineToDescription("Hypnotic Drive", "Test");
    // batch.UpdateMacroCcs("Hypnotic Drive", "Test");
    // batch.ChangeDelayToZero("Hypnotic Drive", "Test");
    // batch.ChangeReverbToZero("Hypnotic Drive", "Test");
    // batch.ReplaceModWheelWithMacro("Hypnotic Drive", "Test");
    // batch.OptimiseWheelMacro("Hypnotic Drive", "Test");
    //
    // batch.OptimiseWheelMacro(null);
    // batch.PrependPathLineToDescription("Factory", "Lo-Fi 2.5");
    // batch.PrependPathLineToDescription("Factory", "RetroWave 2.5");
    // batch.PrependPathLineToDescription("Factory", "VCF-20 Synths 2.5");
    // batch.ReplaceModWheelWithMacro("Factory", "Lo-Fi 2.5");
    // batch.ReplaceModWheelWithMacro("Factory", "RetroWave 2.5");
    // batch.ReplaceModWheelWithMacro("Factory", "VCF-20 Synths 2.5");
    // batch.RollForward("Factory", "RetroWave 2.5");
    // batch.RollForward("Factory", "VCF-20 Synths 2.5");
    // batch.RestoreOriginal("Factory", "Lo-Fi 2.5");
    // batch.RestoreOriginal("Factory", "RetroWave 2.5");
    // batch.RestoreOriginal("Factory", "VCF-20 Synths 2.5");
    // batch.PrependPathLineToDescription(null);
    // batch.PrependPathLineToDescription("Titanium", "Basses");
    // batch.RollForward();
    // batch.UpdateMacroCcs(null);
    // batch.ChangeDelayToZero(null);
    // batch.ChangeReverbToZero("Savage", "Pads-Drones"); 
    // batch.RestoreOriginal(null);
    // batch.ReplaceModWheelWithMacro(null);
    // batch.ReplaceModWheelWithMacro("Factory", "Test");
    // batch.ReplaceModWheelWithMacro("Factory");
    // batch.ReplaceModWheelWithMacro("Titanium");
    // batch.RemoveInfoPageCcsScriptProcessor("Factory", "Test");
    // batch.RemoveInfoPageCcsScriptProcessor("Titanium", "Test");
    // batch.ListIfHasInfoPageCcsScriptProcessor(null);
    // batch.UpdateMacroCcs("Devinity", "Test"); // Still OK
    // batch.ChangeDelayToZero("Devinity", "Test"); // Still OK
    // batch.ReplaceModWheelWithMacro("Devinity", "Test"); // Still OK
    // batch.ChangeReverbToZero("Devinity", "Test"); // Fixed!
    // batch.RestoreOriginal(null);
    // batch.UpdateMacroCcs(null);
    // batch.ChangeDelayToZero(null);
    // batch.ReplaceModWheelWithMacro(null);
    // batch.ChangeReverbToZero(null);
    // batch.UpdateMacroCcs("Factory");
    // batch.ChangeDelayToZero("Factory");
    // batch.ChangeReverbToZero("Factory");
    // batch.UpdateMacroCcs("Spectre");
    // batch.ChangeDelayToZero("Spectre");
    // batch.ChangeReverbToZero("Spectre");
    // batch.ReplaceModWheelWithMacro(null);
    // batch.ReplaceModWheelWithMacro("Spectre");
    // batch.ReplaceModWheelWithMacro("Factory", "Temp");
    // batch.UpdateMacroCcs("Factory", "Temp");
    // batch.ReplaceModWheelWithMacro("Factory", "Temp");
    // batch.ChangeDelayToZero("Factory", "Pure Additive 2.0");
    // batch.ChangeReverbToZero("Factory", "Pure Additive 2.0");
    // batch.ChangeDelayToZero(null);
    // batch.ChangeReverbToZero(null);
    // batch.ChangeDelayToZero("Devinity", "Plucks-Leads");
    // batch.ChangeReverbToZero("Devinity", "Plucks-Leads");
    // batch.ChangeMacroCcNo(38, 28, null);
    // batch.ChangeDelayToZero(null);
    // batch.ChangeReverbToZero(null);
    // batch.ChangeDelayToZero("Devinity");
    // batch.ChangeDelayToZero("Eternal Funk");
    // batch.ChangeDelayToZero("Ether Fields");
    // batch.ChangeDelayToZero("Factory");
    // batch.ChangeDelayToZero("Fluidity");
    // batch.ChangeDelayToZero("Hypnotic Drive"); // None
    // batch.ChangeDelayToZero("Inner Dimensions"); // None
    // batch.ChangeDelayToZero("Pulsar");
    // batch.ChangeDelayToZero("Savage");
    // batch.ChangeDelayMacroValueToZero("Spectre");
    // batch.ChangeDelayMacroValueToZero("Titanium");
    // batch.ChangeDelayMacroValueToZero("Voklm"); // None
    // batch.UpdateMacroCcs("Devinity");
    // batch.UpdateMacroCcs("Eternal Funk");
    // batch.UpdateMacroCcs("Ether Fields");
    // batch.UpdateMacroCcs("Factory");
    // batch.UpdateMacroCcs("Fluidity");
    // batch.UpdateMacroCcs("Titanium");
    //
    // batch.UpdateMacroCcs("Savage");
    // batch.UpdateMacroCcs("Voklm");
    // batch.UpdateMacroCcs("Eternal Funk");
    // batch.UpdateMacroCcs("Ether Fields");
    // batch.UpdateMacroCcs("Inner Dimensions");
    // batch.CountMacros("Fluidity");
    // batch.UpdateMacroCcs("Fluidity");
    // batch.UpdateMacroCcs("Pulsar", "Plucks");
    // batch.UpdateMacroCcs("Pulsar", "Pads");
    // batch.UpdateMacroCcs("Pulsar", "Leads");
    // batch.ChangeMacroCcNo(38, 28, "Factory");
    // batch.ChangeMacroCcNo(38, 28, "Devinity");
    // batch.UpdateMacroCcs("Organic Keys");
    // batch.UpdateMacroCcs("Pulsar", "Bass");
    // batch.ReplaceModWheelWithMacro("Devinity");
    // batch.UpdateMacroCcs("Devinity");
    // batch.ReplaceModWheelWithMacro("Factory", "Test");
    // batch.ReplaceModWheelWithMacro("Factory");
    // batch.ReplaceModWheelWithMacro("Factory", "Brutal Bass 2.1");
  }
}