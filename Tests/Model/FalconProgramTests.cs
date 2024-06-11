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
  public void InitialiseOrganicPadsProgram() {
    Batch.EmbeddedProgramFileName = "Tibetan Horns.xml";
    Batch.EmbeddedTemplateFileName = "Crystal Caves.xml";
    Batch.RunTask(ConfigTask.InitialiseLayout,
      "Organic Pads", "Mystical", "Tibetan Horns");
    Assert.That(Batch.TestProgram.SavedXml, Does.Contain("<script><![CDATA["));
  }

  [Test]
  public void PrependPathLineToDescription() {
    // Program.Properties.description attribute already exists
    PrependPathForEmbeddedFile("Voltage.xml");
    // Program.Properties.description attribute must be added.
    PrependPathForEmbeddedFile("NoGuiScriptProcessor.xml");
    // Program.Properties element must be added with description attribute.
    PrependPathForEmbeddedFile("GuiScriptProcessor.xml", "Pulsar");
    return;

    void PrependPathForEmbeddedFile(string embeddedProgramFileName,
      string soundBankName = "Falcon Factory") {
      Batch.MockBatchLog.Lines.Clear();
      Batch.EmbeddedProgramFileName = embeddedProgramFileName;
      Batch.RunTask(ConfigTask.PrependPathLineToDescription,
        soundBankName, "Bass", "Imagination");
      Assert.That(Batch.TestProgram.SavedXml, Does.Contain(
        @$"PATH: {soundBankName}\Bass\Imagination"));
      Assert.That(Batch.MockBatchLog.Lines[0], Is.EqualTo(
        @$"PrependPathLineToDescription - {soundBankName}\Bass\Imagination: " +
        "Prepended path line to description."));
    }
  }

  [Test]
  public void RestoreOriginal() {
    Batch.RunTask(ConfigTask.RestoreOriginal,
      "Falcon Factory", "Bass", "Imagination");
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Restored to Original"));
  }

  [Test]
  public void RestoreOriginalCannotFindOriginalFile() {
    const string soundBankName = "Falcon Factory";
    const string categoryName = "Bass";
    const string programName = "Imagination";
    string programPath = Path.Combine(Batch.Settings.ProgramsFolder.Path,
      soundBankName, categoryName, $"{programName}.uvip");
    Batch.MockFileSystemService.File.ExistingPaths.Add(programPath);
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.RestoreOriginal,
        soundBankName, categoryName, programName));
    Assert.That(exception.Message, Does.StartWith("Cannot find original file"));
  }

  [Test]
  public void EmbeddedGuiTemplate() {
    const string soundBankName = "Falcon Factory";
    // This category tests Modulation.FixToggleOrContinuous
    const string categoryName = "Brutal Bass 2.1";
    // This program requires Modulation.FixToggleOrContinuous to correct
    // the fourth MIDI CC number from the one provided by the template.
    const string programName = "Magnetic 1";
    Batch.RunTask(ConfigTask.UpdateMacroCcs, soundBankName, categoryName, programName);
    Assert.That(Batch.TestProgram.SavedXml, Does.Contain("@MIDI CC 34"));
  }

  [Test]
  public void UpdateMacroCcsGuiScriptProcessorNotSupportedForSoundBank() {
    const string soundBankName = "Falcon Factory";
    const string categoryName = "Leads";
    const string programName = "Soft Mood";
    Batch.Settings.MustUseGuiScriptProcessorCategories.Add(
      new Settings.SoundBankCategory {
        SoundBank = soundBankName,
        Category = categoryName
      });
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.UpdateMacroCcs, soundBankName, categoryName, programName));
    Assert.That(exception.Message, Does.StartWith(
      "Updating the macro MIDI CCs of a program with a GUI script processor " +
      "is not supported for sound bank"));
  }

  [Test]
  public void UpdateMacroCcsMissingCcNos() {
    Batch.Settings.MidiForMacros = new MidiForMacros();
    const string soundBankName = "Falcon Factory";
    const string categoryName = "Brutal Bass 2.1";
    const string programName = "Magnetic 1";
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.UpdateMacroCcs, soundBankName, categoryName, programName));
    Assert.That(exception.Message, Does.StartWith(
      "The MIDI CC numbers assigned to macros cannot be updated"));
  }

  [Test]
  public void UpdateModulationsFromTemplate() {
    const string soundBankName = "Falcon Factory";
    // This category tests Modulation.FixToggleOrContinuous
    const string categoryName = "Brutal Bass 2.1";
    // This program requires Modulation.FixToggleOrContinuous to correct
    // the fourth MIDI CC number from the one provided by the template.
    const string programName1 = "Magnetic 1";
    // This program requires Modulation.FixToggleOrContinuous to leave
    // the fourth MIDI CC number unaltered as the one provided by the template.
    // This test proves that the result is not affected (any more!) by fixing the
    // previous program.
    const string programName2 = "World Up";
    const string templateProgramName = "808 Line";
    string categoryFolderPath = Path.Combine(
      Batch.Settings.ProgramsFolder.Path, soundBankName, categoryName);
    List<string> programNames = [programName1, programName2];
    Batch.MockFileSystemService.Folder.SimulatedFilePaths.Add(
      categoryFolderPath, programNames);
    string categoryTemplateFolderPath = Path.Combine(
      Batch.Settings.TemplateProgramsFolder.Path, soundBankName, categoryName);
    Batch.MockFileSystemService.Folder.SimulatedFilePaths.Add(
      categoryTemplateFolderPath, [$"{templateProgramName}.uvp"]);
    Batch.EmbeddedTemplateFileName = $"{templateProgramName}.xml";
    Batch.ProgramConfigured += (_, _) => {
      switch (Batch.TestProgram.Name) {
        case programName1:
          Assert.That(Batch.TestProgram.SavedXml, Does.Contain(programName1));
          Assert.That(Batch.TestProgram.GuiScriptProcessor!.Modulations[3].CcNo!.Value,
            Is.EqualTo(34));
          Assert.That(Batch.TestProgram.SavedXml, Does.Contain("@MIDI CC 34"));
          break;
        case programName2:
          Assert.That(Batch.TestProgram.SavedXml, Does.Contain(programName2));
          Assert.That(Batch.TestProgram.GuiScriptProcessor!.Modulations[3].CcNo!.Value,
            Is.EqualTo(112));
          Assert.That(Batch.TestProgram.SavedXml, Does.Contain("@MIDI CC 112"));
          break;
      }
    };
    Batch.RunTask(ConfigTask.UpdateMacroCcs, soundBankName, categoryName);
  }
}