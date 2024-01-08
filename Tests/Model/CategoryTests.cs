using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

#pragma warning disable NUnit2005 // Consider using Assert.That(actual, Is.EqualTo(expected)) instead of Assert.AreEqual(expected, actual)
[TestFixture]
public class CategoryTests {
  [Test]
  public void CategoryFolderDoesNotExist() {
    var category =
      new Category(GetSoundBankFolder("Pulsar"), "DoesNotExist", ReadSettings());
    Assert.Throws<InvalidOperationException>(() => category.Initialise());
  }

  [Test]
  public void FactoryCategorySpecificTemplate() {
    var category = new Category(GetSoundBankFolder("Factory"), 
      "Organic Texture 2.8", ReadSettings());
    category.Initialise();
    Assert.IsTrue(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Organic Texture 2.8", category.TemplateCategoryName);
    Assert.AreEqual("BAS Biggy", category.TemplateProgramName);
  }

  [Test]
  public void FactoryDefaultTemplate() {
    var category =
      new Category(GetSoundBankFolder("Factory"), "Bass-Sub", ReadSettings());
    category.Initialise();
    Assert.IsFalse(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void Main() {
    var category =
      new Category(GetSoundBankFolder("Fluidity"), "Electronic", ReadSettings());
    category.Initialise();
    Assert.IsFalse(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Electronic", category.Name);
    Assert.AreEqual("Fluidity", category.SoundBankFolder.Name);
    Assert.AreEqual("Fluidity", category.TemplateSoundBankName);
    Assert.AreEqual("Strings", category.TemplateCategoryName);
    Assert.AreEqual("Guitar Stream", category.TemplateProgramName);
    Assert.AreEqual(
      Path.Combine(
        SettingsTestHelper.TemplateProgramsFolderPath, "Fluidity", "Strings",
        "Guitar Stream.uvip"),
      category.TemplateProgramPath);
    Assert.IsNotNull(category.TemplateScriptProcessor);
    Assert.That(category.GetProgramFilesToEdit().Any());
  }

  [Test]
  public void NonFactoryDefaultTemplateSameAsFactory() {
    // ReSharper disable once StringLiteralTypo
    var category =
      new Category(GetSoundBankFolder("Spectre"), "Polysynth", ReadSettings());
    category.Initialise();
    Assert.IsFalse(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void PulsarHasCategorySpecificTemplates() {
    var category = new Category(GetSoundBankFolder("Pulsar"), "Bass", ReadSettings());
    category.Initialise();
    Assert.IsTrue(category.MustUseGuiScriptProcessor);
    Assert.AreEqual("Pulsar", category.TemplateSoundBankName);
    Assert.AreEqual("Bass", category.TemplateCategoryName);
    Assert.AreEqual("Warped", category.TemplateProgramName);
  }

  [Test]
  public void SoundBankFolderDoesNotExist() {
    var category =
      new Category(GetSoundBankFolder("DoesNotExist"), "Bass", ReadSettings());
    Assert.Throws<InvalidOperationException>(() => category.Initialise());
  }

  private static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var result = new DirectoryInfo(
      Path.Combine(
        SettingsTestHelper.ProgramsFolderPath, soundBankName));
    return result;
  }

  private static Settings ReadSettings() {
    var settingsFolderLocationReader = new SettingsFolderLocationReader(
      FileSystemService.Default, Serialiser.Default);
    var settingsFolderLocation = settingsFolderLocationReader.Read();
    settingsFolderLocation.Path = Settings.DefaultSettingsFolderPath;
    settingsFolderLocation.Write();
    var settingsReader = new SettingsReader(
      FileSystemService.Default, Serialiser.Default);
    return settingsReader.Read();
  }
}