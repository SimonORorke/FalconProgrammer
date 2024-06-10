using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class CategoryTests {
  [SetUp]
  public void Setup() {
    var reader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "BatchSettings.xml"
    };
    Settings = reader.Read();
  }

  private Settings Settings { get; set; } = null!;

  [Test]
  public void CategoryFolderDoesNotExist() {
    var category =
      new TestCategory("Pulsar", "DoesNotExist", Settings) {
        MockFileSystemService = {
          Folder = {
            SimulatedExists = false
          }
        }
      };
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void FactoryCategorySpecificTemplate() {
    var category = new TestCategory("Falcon Factory",
      "Organic Texture 2.8", Settings) {
      EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
    };
    category.ConfigureMockFileSystemService(
      @"Falcon Factory\Organic Texture 2.8",
      "BAS Biggy.uvip");
    category.Initialise();
    Assert.That(category.MustUseGuiScriptProcessor);
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Falcon Factory"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Organic Texture 2.8"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("BAS Biggy"));
  }

  [Test]
  public void FactoryDefaultTemplate() {
    var category =
      new TestCategory("Falcon Factory", "Bass-Sub", Settings);
    category.ConfigureMockFileSystemService(
      @"Falcon Factory\Keys", "DX Mania.uvip");
    category.Initialise();
    Assert.That(!category.MustUseGuiScriptProcessor);
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Falcon Factory"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Keys"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("DX Mania"));
  }

  [Test]
  public void Main() {
    var category =
      new TestCategory("Fluidity", "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
      };
    category.ConfigureMockFileSystemService(
      @"Fluidity\Strings", "Guitar Stream.uvip");
    category.MockFileSystemService.Folder.SimulatedFilePaths.Add(
      category.Path, ["Cream Synth.uvip", "Fluid Sweeper.uvip"]);
    category.Initialise();
    Assert.That(!category.MustUseGuiScriptProcessor);
    Assert.That(category.Name, Is.EqualTo("Electronic"));
    Assert.That(category.SoundBankName, Is.EqualTo("Fluidity"));
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Fluidity"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Strings"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("Guitar Stream"));
    Assert.That(category.TemplateProgramPath, Is.EqualTo(Path.Combine(
      Settings.TemplateProgramsFolder.Path, "Fluidity", "Strings",
      "Guitar Stream.uvip")));
    Assert.That(category.GetPathsOfProgramFilesToEdit().Any());
  }

  [Test]
  public void NoProgramFilesToEdit() {
    var category =
      new TestCategory("Fluidity", "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
      };
    var exception = Assert.Catch<ApplicationException>(
      () => category.GetPathsOfProgramFilesToEdit());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.Contain(
      "There are no program files to edit in folder"));
  }

  [Test]
  public void ProgramDoesNotExist() {
    var category =
      new TestCategory("Fluidity", "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.xml",
        MockFileSystemService = {
          File = {
            SimulatedExists = false
          }
        }
      };
    var exception = Assert.Catch<ApplicationException>(
      () => category.GetProgramPath("Blah"));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.Contain("Cannot find program file"));
  }

  [Test]
  public void ProgramExists() {
    var category =
      new TestCategory("Fluidity", "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
      };
    Assert.DoesNotThrow(
      () => category.GetProgramPath("Blah"));
  }

  [Test]
  public void ProgramXml() {
    var category =
      new TestCategory("Fluidity", "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
      };
    var programXml = new ProgramXml(category);
    category.ProgramXml = programXml;
    Assert.That(category.ProgramXml.Category.Name, Is.EqualTo(category.Name));
  }

  [Test]
  public void PulsarHasCategorySpecificTemplates() {
    var category = new TestCategory("Pulsar", "Bass", Settings) {
      EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
    };
    category.ConfigureMockFileSystemService(
      @"Pulsar\Bass", "Warped.uvip");
    category.Initialise();
    Assert.That(category.MustUseGuiScriptProcessor);
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Pulsar"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Bass"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("Warped"));
  }

  [Test]
  public void SoundBankFolderDoesNotExist() {
    var category =
      new TestCategory("DoesNotExist", "Bass", Settings) {
        MockFileSystemService = {
          Folder = {
            SimulatedExists = false
          }
        }
      };
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void TemplateProgramsFolderDoesNotExist() {
    var category =
      new TestCategory("Fluidity", "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.xml"
      };
    category.ConfigureMockFileSystemService(
      @"Fluidity\Strings", "Guitar Stream.uvip");
    category.MockFileSystemService.Folder.ExistingPaths.Remove(
      Settings.TemplateProgramsFolder.Path);
    var exception = Assert.Catch<ApplicationException>(
      () => category.Initialise());
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.StartWith(
      "Cannot find template programs folder"));
  }

  [Test]
  public void TemplateScriptProcessorFoundInEmbeddedFile() {
    LookForEmbeddedFile("Falcon Factory", "Brutal Bass 2.1",
      "Magnetic 1");
    LookForEmbeddedFile("Modular Noise", "Bass",
      "Voltage");
    return;

    void LookForEmbeddedFile(string soundBankName, string categoryName, 
      string programName) {
      var category = new TestCategory(soundBankName, categoryName, Settings);
      var batch = new TestBatch();
      var program = new TestFalconProgram(
        $"{programName}.xml", "Will be ignored.uvip", category, batch);
      program.Read();
      Assert.That(program.GuiScriptProcessor, Is.Not.Null);
      Assert.DoesNotThrow(() =>
        category.GetTemplateScriptProcessor(program.GuiScriptProcessor, batch));
    }
  }

  [Test]
  public void TemplateScriptProcessorFoundInFile() {
    // Specify a template program path in the constructor to make
    // GetTemplateScriptProcessor look for the template script processor in the file. 
    var category = new TestCategory("Falcon Factory",
      "Organic Texture 2.8", Settings, "Will be ignored.uvip") {
      EmbeddedTemplateFileName = "BAS Biggy.xml"
    };
    var batch = new TestBatch();
    var program = new TestFalconProgram(
      "KEY Clockworks.xml", "Will also be ignored.uvip", category, batch);
    program.Read();
    Assert.That(program.GuiScriptProcessor, Is.Not.Null);
    Assert.DoesNotThrow(() =>
      category.GetTemplateScriptProcessor(program.GuiScriptProcessor, batch));
  }

  // [Test]
  // public void TemplateScriptProcessorNotFoundInEmbeddedFile() {
  //   var category = new TestCategory("Falcon Factory",
  //     "Organic Texture 2.8", Settings);
  //   var batch = new TestBatch();
  //   var program = new TestFalconProgram(
  //     "KEY Clockworks.xml", "Will also be ignored.uvip", category, batch);
  //   program.Read();
  //   Assert.That(program.GuiScriptProcessor, Is.Not.Null);
  //   var exception = Assert.Catch(() =>
  //     category.GetTemplateScriptProcessor(program.GuiScriptProcessor, batch));
  //   Assert.That(exception, Is.Not.Null);
  //   Assert.That(exception.Message, Does.EndWith(
  //     "Cannot find GUI ScriptProcessor template."));
  // }

  [Test]
  public void TemplateScriptProcessorNotFoundInFile() {
    // Specify a template program path in the constructor to make
    // GetTemplateScriptProcessor look for the template script processor in the file. 
    var category = new TestCategory("Falcon Factory",
      "Organic Texture 2.8", Settings, "Will be ignored.uvip") {
      EmbeddedTemplateFileName = "NoGuiScriptProcessor.xml"
    };
    var batch = new TestBatch();
    var program = new TestFalconProgram(
      "KEY Clockworks.xml", "Will also be ignored.uvip", category, batch);
    program.Read();
    Assert.That(program.GuiScriptProcessor, Is.Not.Null);
    var exception = Assert.Catch(() =>
      category.GetTemplateScriptProcessor(program.GuiScriptProcessor, batch));
    Assert.That(exception, Is.Not.Null);
    Assert.That(exception.Message, Does.Contain(
      "Cannot find the GUI ScriptProcessor in template program file '"));
  }
}