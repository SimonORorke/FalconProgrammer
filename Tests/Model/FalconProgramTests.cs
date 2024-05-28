using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   Each Test here uses an easy way to test <see cref="FalconProgram " />,
///   via <see cref="TestBatch" />. Tests of <see cref="Batch" /> where program
///   updates are mocked out should go in <see cref="BatchTests" />.
/// </summary>
/// <remarks>
///   There are few tests of <see cref="FalconProgram " /> here because the easiest way
///   to regression test it is to run an actual batch and then check in the Program
///   folder's version control to confirm that there have been no changes. A
///   comprehensive test is to use <see cref="BatchRunner" /> to run
///   <see cref="Batch.RollForward" /> for all sound banks.
/// </remarks>
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
    Batch.EmbeddedProgramFileName = "Tibetan Horns.xml";
    Batch.EmbeddedTemplateFileName = "Crystal Caves.uvip";
    SoundBankBackground("Organic Pads", "Bluish Teal.png");
    return;

    void SoundBankBackground(string soundBankName,
      string expectedImageFileName) {
      const string categoryName = "Bass";
      const string programName = "Imagination";
      Batch.MockBatchLog.Lines.Clear();
      Batch.RunTask(
        ConfigTask.InitialiseLayout, soundBankName, categoryName, programName);
      string expectedPath = $"\"./../../../Images/{expectedImageFileName}\"";
      Assert.That(Batch.TestProgram.SavedXml, Does.Contain(expectedPath));
      Assert.That(Batch.MockBatchLog.Text, Does.Contain("Set BackgroundImagePath"));
    }
  }

  [Test]
  public void PrependPathLineToDescription() {
    Batch.EmbeddedProgramFileName = "Voltage.xml";
    Batch.RunTask(ConfigTask.InitialiseLayout, 
      "Factory", "Bass", "Imagination");
    string programText = Batch.TestProgram.ProgramTextWriter!.ToString()!; 
    Assert.That(programText, Does.Contain("Blah"));
  }

  [Test]
  public void RestoreOriginal() {
    Batch.RunTask(ConfigTask.RestoreOriginal,
      "Factory", "Bass", "Imagination");
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Restored to Original"));
  }

  [Test]
  public void RestoreOriginalCannotFindOriginalFile() {
    Batch.MockFileSystemService.File.SimulatedExists = false;
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.RestoreOriginal,
        "Factory", "Bass", "Imagination"));
    Assert.That(exception.Message, Does.StartWith("Cannot find original file"));
  }
}