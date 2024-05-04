﻿namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class FalconProgramTests {
  [SetUp]
  public void Setup() {
    Batch = new TestBatch {
      TestSettingsReaderEmbedded = {
        EmbeddedFileName = "BatchSettings.xml"
      }
    };
  }

  private TestBatch Batch { get; set; } = null!;

  [Test]
  public void Background() {
    SoundBankBackground("Eternal Funk", "Yellowish Mid-Green.png");
    SoundBankBackground("Fluidity", "Midnight Blue.png");
    SoundBankBackground("Hypnotic Drive", "Dark Goldenrod.png");
    SoundBankBackground("Inner Dimensions", "Dark Red.png");
    SoundBankBackground("Modular Noise", "Dark Forest.png");
    SoundBankBackground("Savage", "Heath.png");
    SoundBankBackground("Titanium", "Dull Purple.png");
    return;

    void SoundBankBackground(string soundBankName,
      string expectedImageFileName) {
      const string categoryName = "Bass";
      const string programName = "Imagination";
      Batch.MockBatchLog.Lines.Clear();
      Batch.InitialiseLayout(soundBankName, categoryName, programName);
      Assert.That(Batch.TestProgram.TestProgramXml.SavedXml, Does.Contain(
        expectedImageFileName));
      Assert.That(Batch.MockBatchLog.Text, Does.Contain("Set BackgroundImagePath"));
    }
  }
}