using FalconProgrammer.Tests.Model;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests;

internal static class TestHelper {
  public static void AddSoundBankSubfolders(
    MockFolderService mockFolderService, string folderPath) {
    mockFolderService.ExpectedSubfolderNames.Add(
      folderPath, [
        "Ether Fields", "Factory", "Organic Keys", "Pulsar", "Spectre", "Voklm"
      ]);
    mockFolderService.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Ether Fields"), [
        "Granular", "Hybrid"
      ]);
    mockFolderService.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Factory"), [
        "Bass-Sub", "Keys", "Organic Texture 2.8"
      ]);
    mockFolderService.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Organic Keys"), [
        "Acoustic Mood", "Lo-Fi"
      ]);
    mockFolderService.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Pulsar"), [
        "Bass", "Leads", "Plucks"
      ]);
    mockFolderService.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Spectre"), [
        "Bells", "Chords"
      ]);
    mockFolderService.ExpectedSubfolderNames.Add(
      Path.Combine(folderPath, "Voklm"), [
        "Synth Choirs", "Vox Instruments"
      ]);
  }

  public static CcNoRangeViewModel CreateCcNoRangeAdditionItem(int? start, int? end) {
    return new CcNoRangeViewModel(true) {
      Start = start,
      End = end
    };
  }
}