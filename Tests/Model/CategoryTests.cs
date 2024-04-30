using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

[TestFixture]
public class CategoryTests {
  [SetUp]
  public void Setup() {
    var reader = new TestSettingsReaderEmbedded {
      EmbeddedFileName = "LocationsSettings.xml"
    };
    Settings = reader.Read();
  }

  private Settings Settings { get; set; } = null!;

  [Test]
  public void CannotFindTemplateScriptProcessor() {
    var category = new TestCategory(GetSoundBankFolderName("Factory"),
      "Organic Texture 2.8", Settings) {
      EmbeddedTemplateFileName = "NoGuiScriptProcessor.uvip"
    };
    category.ConfigureMockFileSystemService(
      @"Factory\Organic Texture 2.8",
      "BAS Biggy.uvip");
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void CategoryFolderDoesNotExist() {
    var category =
      new TestCategory(GetSoundBankFolderName("Pulsar"), "DoesNotExist", Settings) {
        MockFileSystemService = {
          Folder = {
            ExpectedExists = false
          }
        }
      };
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void FactoryCategorySpecificTemplate() {
    var category = new TestCategory(GetSoundBankFolderName("Factory"),
      "Organic Texture 2.8", Settings) {
      EmbeddedTemplateFileName = "GuiScriptProcessor.uvip"
    };
    category.ConfigureMockFileSystemService(
      @"Factory\Organic Texture 2.8",
      "BAS Biggy.uvip");
    category.Initialise();
    Assert.That(category.MustUseGuiScriptProcessor);
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Factory"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Organic Texture 2.8"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("BAS Biggy"));
  }

  [Test]
  public void FactoryDefaultTemplate() {
    var category =
      new TestCategory(GetSoundBankFolderName("Factory"), "Bass-Sub", Settings);
    category.ConfigureMockFileSystemService(
      @"Factory\Keys", "DX Mania.uvip");
    category.Initialise();
    Assert.That(!category.MustUseGuiScriptProcessor);
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Factory"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Keys"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("DX Mania"));
  }

  [Test]
  public void Main() {
    var category =
      new TestCategory(GetSoundBankFolderName("Fluidity"), "Electronic", Settings) {
        EmbeddedTemplateFileName = "GuiScriptProcessor.uvip"
      };
    category.ConfigureMockFileSystemService(
      @"Fluidity\Strings", "Guitar Stream.uvip");
    category.MockFileSystemService.Folder.ExpectedFilePaths.Add(
      category.CategoryFolderPath, ["Cream Synth.uvip", "Fluid Sweeper.uvip"]);
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
    Assert.That(category.TemplateScriptProcessor, Is.Not.Null);
  }

  [Test]
  public void NonFactoryDefaultTemplateSameAsFactory() {
    // ReSharper disable once StringLiteralTypo
    var category =
      new TestCategory(GetSoundBankFolderName("Spectre"), "Polysynth", Settings);
    category.ConfigureMockFileSystemService(
      @"Factory\Keys", "DX Mania.uvip");
    category.Initialise();
    Assert.That(!category.MustUseGuiScriptProcessor);
    Assert.That(category.TemplateSoundBankName, Is.EqualTo("Factory"));
    Assert.That(category.TemplateCategoryName, Is.EqualTo("Keys"));
    Assert.That(category.TemplateProgramName, Is.EqualTo("DX Mania"));
  }

  [Test]
  public void PulsarHasCategorySpecificTemplates() {
    var category = new TestCategory(GetSoundBankFolderName("Pulsar"), "Bass", Settings) {
      EmbeddedTemplateFileName = "GuiScriptProcessor.uvip"
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
      new TestCategory(GetSoundBankFolderName("DoesNotExist"), "Bass", Settings) {
        MockFileSystemService = {
          Folder = {
            ExpectedExists = false
          }
        }
      };
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  private string GetSoundBankFolderName(string soundBankName) {
    return Path.Combine(Settings.ProgramsFolder.Path, soundBankName);
  }
}