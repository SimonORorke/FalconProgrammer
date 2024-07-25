using FalconProgrammer.Model;
using FalconProgrammer.Model.Mpe;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   Each Test here uses an easy way to test <see cref="FalconProgram " />,
///   via <see cref="TestBatch" />. Tests of <see cref="Batch" /> where program
///   updates are mocked out should go in <see cref="BatchTests" />.
/// </summary>
/// <remarks>
///   There are few tests of <see cref="FalconProgram " /> here because the easiest way
///   to regression test it is to run an actual batch and then check in the Program
///   folder's version control to confirm that there have been no changes.
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
  public void AssignMacroCcsGuiScriptProcessorNotSupportedForCategory() {
    const string soundBankName = "Falcon Factory";
    const string categoryName = "Leads";
    const string programName = "Soft Mood";
    Batch.Settings.MustUseGuiScriptProcessorCategories.Add(
      new SoundBankCategorySetting {
        SoundBank = soundBankName,
        Category = categoryName
      });
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.AssignMacroCcs, soundBankName, categoryName, programName));
    Assert.That(exception.Message, Does.Contain(
      "Assigning MIDI CCs to macros for a program with a GUI script processor " +
      $"is not supported for sound bank {soundBankName} category {categoryName}."));
  }

  [Test]
  public void AssignMacroCcsGuiScriptProcessorNotSupportedForSoundBank() {
    const string soundBankName = "Falcon Factory rev2";
    const string categoryName = "Bass";
    const string programName = "Big Sleep";
    Batch.Settings.MustUseGuiScriptProcessorCategories.Add(
      new SoundBankCategorySetting {
        SoundBank = soundBankName
      });
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.AssignMacroCcs, soundBankName, categoryName, programName));
    Assert.That(exception.Message, Does.StartWith(
      "Assigning MIDI CCs to macros for a program with a GUI script processor " +
      $"is not supported for sound bank {soundBankName}."));
  }

  [Test]
  public void AssignMacroCcsMissingCcNos() {
    Batch.Settings.MidiForMacros = new MidiForMacros();
    const string soundBankName = "Falcon Factory";
    const string categoryName = "Brutal Bass 2.1";
    const string programName = "Magnetic 1";
    var exception = Assert.Catch<ApplicationException>(() =>
      Batch.RunTask(ConfigTask.AssignMacroCcs, soundBankName, categoryName, programName));
    Assert.That(exception.Message, Does.StartWith(
      "MIDI CC numbers cannot be assigned to macros "));
  }

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
  public void EmbeddedGuiTemplate() {
    const string soundBankName = "Falcon Factory";
    // This category tests Modulation.FixToggleOrContinuous
    const string categoryName = "Brutal Bass 2.1";
    // This program requires Modulation.FixToggleOrContinuous to correct
    // the fourth MIDI CC number from the one provided by the template.
    const string programName = "Magnetic 1";
    Batch.RunTask(ConfigTask.AssignMacroCcs, soundBankName, categoryName, programName);
    Assert.That(Batch.TestProgram.SavedXml, Does.Contain("@MIDI CC 34"));
  }

  [Test]
  public void InitialiseOrganicPadsProgram() {
    Batch.EmbeddedProgramFileName = "Tibetan Horns.xml";
    Batch.EmbeddedTemplateFileName = "Crystal Caves.xml";
    Batch.RunTask(ConfigTask.InitialiseLayout,
      "Organic Pads", "Mystical", "Tibetan Horns");
    Assert.That(Batch.TestProgram.SavedXml, Does.Contain("AttackTime=\"0.02\""));
    Assert.That(Batch.TestProgram.SavedXml, Does.Contain("ReleaseTime=\"0.3\""));
    Assert.That(Batch.TestProgram.SavedXml, Does.Contain("<script><![CDATA["));
    Assert.That(Batch.TestProgram.LastWrittenFilePath, Is.EqualTo(
      @"J:\FalconProgrammer\Scripts\DahdsrController\DahdsrController.lua"));
    Assert.That(Batch.TestProgram.LastWrittenFileContents, Does.Contain(
      "MaxAttackSeconds = 1"));
    Assert.That(Batch.TestProgram.LastWrittenFileContents, Does.Contain(
      "MaxDecaySeconds = 15"));
    Assert.That(Batch.TestProgram.LastWrittenFileContents, Does.Contain(
      "MaxReleaseSeconds = 2"));
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
  public void RemoveArpeggiatorsAndSequencing() {
    Run("Falcon Factory rev2", "Bass", "Big Sleep");
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith("Removed Arpeggiator(s)."));
    Assert.That(Batch.MockBatchLog.Lines[1], Does.EndWith(
      "Removed macro Sequence (Macro 17)."));
    Run("Modular Noise", "Bass", "Voltage");
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith("Removed Arpeggiator(s)."));
    Assert.That(Batch.MockBatchLog.Lines[1], Does.EndWith(
      "Removed program-level sequencing script processor(s)."));
    Run("Modular Noise", "Chords", "Buffers (F)");
    Assert.That(Batch.MockBatchLog.Lines[0], Does.EndWith(
      "Removed program-level sequencing script processor(s)."));
    Assert.That(Batch.MockBatchLog.Lines[1], Does.EndWith(
      "Removed below-program-level sequencing script processor(s)."));
    Assert.That(Batch.MockBatchLog.Lines[2], Does.EndWith(
      "Removed macro Rate (Macro 3)."));
    return;

    void Run(string soundBankName, string categoryName, string programName) {
      // Remove GUI script processor first.
      Batch.NextProgramTestXml = null;
      Batch.MockBatchLog.Lines.Clear();
      Batch.RunTask(
        ConfigTask.InitialiseLayout, soundBankName, categoryName, programName);
      Batch.NextProgramTestXml = Batch.TestProgram.SavedXml;
      Batch.MockBatchLog.Lines.Clear();
      Batch.RunTask(
        ConfigTask.RemoveArpeggiatorsAndSequencing, soundBankName, categoryName, 
        programName);
    }
  }

  [Test]
  public void RestoreOriginal() {
    Batch.RunTask(ConfigTask.RestoreOriginal,
      "Falcon Factory", "Bass", "Imagination");
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Restored to original."));
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
  public void SupportMpe1ContinuousMacro() {
    const string soundBankName = "Falcon Factory rev2";
    const string categoryName = "Polysynth";
    const string programName = "Drama Queen";
    Batch.RunTask(ConfigTask.SupportMpe, soundBankName, categoryName, programName);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Added MPE support."));
    bool found = Batch.TestProgram.TryGetMpeScriptProcessor(out var mpeScriptProcessor);
    Assert.That(found, Is.True);
    Assert.That(mpeScriptProcessor!.XTarget, Is.EqualTo(XTarget.Pitch));
    Assert.That(mpeScriptProcessor.ZTarget, Is.EqualTo(ZTarget.Gain));
    Assert.That(mpeScriptProcessor.YTarget, Is.EqualTo(YTarget.ContinuousMacro1Bipolar));
  }

  [Test]
  public void SupportMpe2ContinuousMacros() {
    const string soundBankName = "Falcon Factory rev2";
    const string categoryName = "Polysynth";
    const string programName = "Guitar Distortion Booth";
    Batch.RunTask(ConfigTask.SupportMpe, soundBankName, categoryName, programName);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Added MPE support."));
    bool found = Batch.TestProgram.TryGetMpeScriptProcessor(out var mpeScriptProcessor);
    Assert.That(found, Is.True);
    Assert.That(mpeScriptProcessor!.XTarget, Is.EqualTo(XTarget.Pitch));
    Assert.That(mpeScriptProcessor.ZTarget, Is.EqualTo(ZTarget.ContinuousMacro2Unipolar));
    Assert.That(mpeScriptProcessor.YTarget, Is.EqualTo(YTarget.ContinuousMacro1Bipolar));
  }

  [Test]
  public void SupportMpe3ContinuousMacros() {
    const string soundBankName = "Falcon Factory rev2";
    const string categoryName = "Polysynth";
    const string programName = "House Classic Gate";
    Batch.RunTask(ConfigTask.SupportMpe, soundBankName, categoryName, programName);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Added MPE support."));
    bool found = Batch.TestProgram.TryGetMpeScriptProcessor(out var mpeScriptProcessor);
    Assert.That(found, Is.True);
    Assert.That(mpeScriptProcessor!.XTarget, Is.EqualTo(XTarget.ContinuousMacro3Bipolar));
    Assert.That(mpeScriptProcessor.ZTarget, Is.EqualTo(ZTarget.ContinuousMacro2Unipolar));
    Assert.That(mpeScriptProcessor.YTarget, Is.EqualTo(YTarget.ContinuousMacro1Bipolar));
    Assert.That(Batch.TestProgram.Macros[0].ModulatedConnectionsParents, 
      Has.Count.EqualTo(1));
    Assert.That(Batch.TestProgram.Macros[1].ModulatedConnectionsParents, 
      Has.Count.EqualTo(1));
    Assert.That(Batch.TestProgram.Macros[0].ModulatedConnectionsParents, 
      Is.EqualTo(Batch.TestProgram.Macros[1].ModulatedConnectionsParents));
    Assert.That(Batch.TestProgram.Macros[2].ModulatedConnectionsParents, 
      Has.Count.EqualTo(1));
    Assert.That(
      Batch.TestProgram.Macros[0].ModulatedConnectionsParents[0].Modulations[^2].Source, 
      Is.EqualTo("$Program/MPE Y Modulation"));
    Assert.That(Batch.TestProgram.Macros[1].ModulatedConnectionsParents, 
      Has.Count.EqualTo(1));
    Assert.That(
      Batch.TestProgram.Macros[1].ModulatedConnectionsParents[0].Modulations[^1].Source, 
      Is.EqualTo("$Program/MPE Z Modulation"));
    Assert.That(Batch.TestProgram.Macros[2].ModulatedConnectionsParents, 
      Has.Count.EqualTo(1));
    Assert.That(
      Batch.TestProgram.Macros[2].ModulatedConnectionsParents[0].Modulations[^1].Source, 
      Is.EqualTo("$Program/MPE X Modulation"));
  }

  [Test]
  public void SupportMpeGuiScriptProcessorExists() {
    const string soundBankName = "Falcon Factory rev2";
    const string categoryName = "MPE";
    const string programName = "Analog Cello";
    Batch.RunTask(ConfigTask.SupportMpe, soundBankName, categoryName, programName);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain(
      "Cannot add MPE support because the program's Info page GUI " + 
      "is specified in a script processor."));
  }

  [Test]
  public void SupportMpeNoContinuousMacros() {
    const string soundBankName = "Falcon Factory";
    const string categoryName = "Keys";
    const string programName = "FM Tremolo";
    Batch.RunTask(ConfigTask.SupportMpe, soundBankName, categoryName, programName);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain("Added MPE support."));
    bool found = Batch.TestProgram.TryGetMpeScriptProcessor(out var mpeScriptProcessor);
    Assert.That(found, Is.True);
    Assert.That(mpeScriptProcessor!.XTarget, Is.EqualTo(XTarget.Pitch));
    Assert.That(mpeScriptProcessor.ZTarget, Is.EqualTo(ZTarget.Gain));
    Assert.That(mpeScriptProcessor.YTarget, Is.EqualTo(YTarget.PolyphonicAftertouch));
  }

  [Test]
  public void SupportMpeScriptProcessorAlreadyExists() {
    const string soundBankName = "Falcon Factory rev2";
    const string categoryName = "MPE";
    const string programName = "Wave Guide";
    Batch.RunTask(ConfigTask.SupportMpe, soundBankName, categoryName, programName);
    Assert.That(Batch.MockBatchLog.Text, Does.Contain(
      "Cannot add MPE support because the program already has an MPE script processor."));
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
    Batch.RunTask(ConfigTask.AssignMacroCcs, soundBankName, categoryName);
  }
}