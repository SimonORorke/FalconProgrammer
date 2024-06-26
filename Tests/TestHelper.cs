﻿using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests;

internal static class TestHelper {
  public static void AddSoundBankSubfolders(
    MockFolderService mockFolderService, string folderPath) {
    mockFolderService.SimulatedSubfolderNames.Add(
      folderPath, [
        "Ether Fields", "Falcon Factory", "Inner Dimensions", "Organic Keys", "Pulsar",
        "Savage",
        "Spectre", "Voklm"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Ether Fields"), [
        "Granular", "Hybrid"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Falcon Factory"), [
        "Bass-Sub", "Keys", "Leads", "Organic Texture 2.8", "Pads", "Synth Brass"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Inner Dimensions"), [
        "Pad"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Organic Keys"), [
        "Acoustic Mood", "Lo-Fi"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Pulsar"), [
        "Bass", "Leads", "Plucks"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Savage"), [
        "Pads-Drones"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Spectre"), [
        "Bells", "Chords", "Leads"
      ]);
    mockFolderService.SimulatedSubfolderNames.Add(
      Path.Combine(folderPath, "Voklm"), [
        "Synth Choirs", "Vox Instruments"
      ]);
  }

  public static CcNoRangeItem CreateCcNoRangeAdditionItem(int? start, int? end) {
    return new CcNoRangeItem(true) {
      Start = start,
      End = end
    };
  }

  /// <summary>
  ///   Waits for a process that is running on another thread to finish.
  /// </summary>
  /// <param name="condition">
  ///   A condition that, when true, will prove that the process has finished.
  /// </param>
  /// <param name="description">
  ///   A description of what we are waiting for, to be shown in an error message if
  ///   the wait times out.
  /// </param>
  /// <param name="maxCount">
  ///   The maximum number of times we should check to ascertain whether the process
  ///   has finished before timing out. Default: 1,000.
  /// </param>
  /// <param name="intervalMilliseconds">
  ///   The interval in milliseconds between checks to ascertain whether the process
  ///   has finished. Default: 1 millisecond.
  /// </param>
  [ExcludeFromCodeCoverage]
  public static void WaitUntil(Func<bool> condition, string description,
    int maxCount = 1000, int intervalMilliseconds = 1) {
    bool finished = false;
    for (int i = 0; i < maxCount; i++) {
      Thread.Sleep(intervalMilliseconds);
      if (condition.Invoke()) {
        finished = true;
        break;
      }
    }
    Assert.That(finished, Is.True, description);
  }
}