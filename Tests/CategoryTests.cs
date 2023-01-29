namespace FalconProgrammer.Tests;

[TestFixture]
public class CategoryTests {
  [Test]
  public void Test1() {
    var category =
      new Category(GetSoundBankFolder("Fluidity"), "Electronic", ReadSettings());
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Electronic", category.Name);
    Assert.AreEqual("Fluidity", category.SoundBankFolder.Name);
    Assert.AreEqual("Fluidity", category.TemplateSoundBankName);
    Assert.AreEqual("Strings", category.TemplateCategoryName);
    Assert.AreEqual("Guitar Stream", category.TemplateProgramName);
    Assert.AreEqual(
      Path.Combine(
        SettingsTestHelper.TemplatesFolderPath, "Fluidity", "Strings",
        "Guitar Stream.uvip"),
      category.TemplateProgramPath);
    Assert.IsNotNull(category.TemplateScriptProcessor);
    Assert.That(category.GetProgramFilesToEdit().Any());
  }

  [Test]
  public void Test2() {
    var category = new Category(GetSoundBankFolder("Factory"), "Brutal Bass 2.1",
      ReadSettings());
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Brutal Bass 2.1", category.TemplateCategoryName);
    Assert.AreEqual("808 Line", category.TemplateProgramName);
  }

  [Test]
  public void Test3() {
    var category =
      new Category(GetSoundBankFolder("Factory"), "Bass-Sub", ReadSettings());
    category.Initialise();
    Assert.IsFalse(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void Test4() {
    var category = new Category(GetSoundBankFolder("Factory"), "RetroWave 2.5",
      ReadSettings());
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Lo-Fi 2.5", category.TemplateCategoryName);
    // ReSharper disable once StringLiteralTypo
    Assert.AreEqual("BAS Gameboy Bass", category.TemplateProgramName);
  }

  [Test]
  public void Test5() {
    // ReSharper disable once StringLiteralTypo
    var category =
      new Category(GetSoundBankFolder("Spectre"), "Polysynth", ReadSettings());
    category.Initialise();
    Assert.IsFalse(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void Test6() {
    var category = new Category(GetSoundBankFolder("Pulsar"), "Bass", ReadSettings());
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Pulsar", category.TemplateSoundBankName);
    Assert.AreEqual("Bass", category.TemplateCategoryName);
    Assert.AreEqual("Warped", category.TemplateProgramName);
  }

  [Test]
  public void Test7() {
    var category = new Category(GetSoundBankFolder("Pulsar"), "Wrong", ReadSettings());
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void Test8() {
    var category = new Category(GetSoundBankFolder("Xyz"), "Bass", ReadSettings());
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void Test9() {
    var settings = ReadSettings();
    var soundBankFolder = GetSoundBankFolder("Pulsar");
    const string categoryName = "Bass";
    var settingsCategory = settings.GetProgramCategory(
      soundBankFolder.Name, categoryName);
    settingsCategory.TemplatePath =
      settingsCategory.TemplatePath.Replace("Warped", "DoesNotExist");
    var category = new Category(soundBankFolder, categoryName, settings);
    Assert.Throws<ApplicationException>(() => category.Initialise());
  }

  [Test]
  public void Test10() {
    var settings = ReadSettings();
    var soundBankFolder = GetSoundBankFolder("Pulsar");
    var settingsCategory = settings.GetProgramCategory(
      soundBankFolder.Name, "Bass");
    const string tempCategoryName = "Empty";
    settingsCategory.Category = tempCategoryName;
    var tempCategory = soundBankFolder.CreateSubdirectory(tempCategoryName);
    try {
      var category = new Category(soundBankFolder, tempCategoryName, settings);
      category.Initialise();
      Assert.Throws<ApplicationException>(() => category.GetProgramFilesToEdit());
    } finally {
      tempCategory.Delete();
    }
  }

  private static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var result = new DirectoryInfo(
      Path.Combine(
        SettingsTestHelper.ProgramsFolderPath, soundBankName));
    return result;
  }

  private static Settings ReadSettings() {
    var settingsFolderLocation = SettingsFolderLocation.Read();
    settingsFolderLocation.Path = Settings.DefaultSettingsFolderPath;
    settingsFolderLocation.Write();
    return Settings.Read();
  }
}