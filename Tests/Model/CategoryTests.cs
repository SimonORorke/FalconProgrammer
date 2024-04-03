using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

#pragma warning disable NUnit2005 // Consider using Assert.That(actual, Is.EqualTo(expected)) instead of Assert.AreEqual(expected, actual)
[TestFixture]
public class CategoryTests {
  [SetUp]
  public void Setup() {
    var reader = new TestSettingsReaderEmbedded {
      TestDeserialiser = {
        EmbeddedResourceFileName = "LocationsSettings.xml"
      }
    };
    Settings = reader.Read();
  }

  private Settings Settings { get; set; } = null!;

  [Test]
  public void CategoryFolderDoesNotExist() {
    var category =
      new TestCategory(GetSoundBankFolder("Pulsar"), "DoesNotExist", Settings) {
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
    var category = new TestCategory(GetSoundBankFolder("Factory"),
      "Organic Texture 2.8", Settings);
    category.ConfigureMockFileSystemService(
      @"Factory\Organic Texture 2.8",
      "BAS Biggy.uvip");
    category.Initialise();
    Assert.IsTrue(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Organic Texture 2.8", category.TemplateCategoryName);
    Assert.AreEqual("BAS Biggy", category.TemplateProgramName);
  }

  [Test]
  public void FactoryDefaultTemplate() {
    var category =
      new TestCategory(GetSoundBankFolder("Factory"), "Bass-Sub", Settings);
    category.ConfigureMockFileSystemService(
      @"Factory\Keys", "DX Mania.uvip");
    category.Initialise();
    Assert.IsFalse(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void Main() {
    var category =
      new TestCategory(GetSoundBankFolder("Fluidity"), "Electronic", Settings);
    category.ConfigureMockFileSystemService(
      @"Fluidity\Strings", "Guitar Stream.uvip");
    category.MockFileSystemService.Folder.ExpectedFilePaths.Add(
      category.CategoryFolderPath, ["Cream Synth.uvip", "Fluid Sweeper.uvip"]);
    category.Initialise();
    Assert.IsFalse(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Electronic", category.Name);
    Assert.AreEqual("Fluidity", category.SoundBankFolder.Name);
    Assert.AreEqual("Fluidity", category.TemplateSoundBankName);
    Assert.AreEqual("Strings", category.TemplateCategoryName);
    Assert.AreEqual("Guitar Stream", category.TemplateProgramName);
    Assert.AreEqual(
      Path.Combine(
        Settings.TemplateProgramsFolder.Path, "Fluidity", "Strings",
        "Guitar Stream.uvip"),
      category.TemplateProgramPath);
    Assert.That(category.GetPathsOfProgramFilesToEdit().Any());
    // Assert.IsNotNull(category.TemplateScriptProcessor);
  }

  [Test]
  public void NonFactoryDefaultTemplateSameAsFactory() {
    // ReSharper disable once StringLiteralTypo
    var category =
      new TestCategory(GetSoundBankFolder("Spectre"), "Polysynth", Settings);
    category.ConfigureMockFileSystemService(
      @"Factory\Keys", "DX Mania.uvip");
    category.Initialise();
    Assert.IsFalse(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void PulsarHasCategorySpecificTemplates() {
    var category = new TestCategory(GetSoundBankFolder("Pulsar"), "Bass", Settings);
    category.ConfigureMockFileSystemService(
      @"Pulsar\Bass", "Warped.uvip");
    category.Initialise();
    Assert.IsTrue(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Pulsar", category.TemplateSoundBankName);
    Assert.AreEqual("Bass", category.TemplateCategoryName);
    Assert.AreEqual("Warped", category.TemplateProgramName);
  }

  [Test]
  public void SoundBankFolderDoesNotExist() {
    var category =
      new TestCategory(GetSoundBankFolder("DoesNotExist"), "Bass", Settings) {
        MockFileSystemService = {
          Folder = {
            ExpectedExists = false
          }
        }
      };
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  private DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var result = new DirectoryInfo(
      Path.Combine(Settings.ProgramsFolder.Path, soundBankName));
    return result;
  }
}