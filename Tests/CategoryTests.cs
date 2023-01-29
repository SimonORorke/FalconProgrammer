namespace FalconProgrammer.Tests; 

[TestFixture]
public class CategoryTests {

  [Test]
  public void Test1() {
    var category = new Category(GetSoundBankFolder("Fluidity"), "Electronic");
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Electronic", category.Name);
    Assert.AreEqual("Fluidity", category.SoundBankFolder.Name);
    Assert.AreEqual("Fluidity", category.TemplateSoundBankName);
    Assert.AreEqual("Strings", category.TemplateCategoryName);
    Assert.AreEqual("Guitar Stream", category.TemplateProgramName);
    // This is to be changed:
    Assert.AreEqual(
      Path.Combine(
        SettingsTestHelper.ProgramsFolderPath, "Fluidity", "Strings", 
        "Guitar Stream.uvip"), 
      category.TemplateProgramPath);
    // To this:
    // Assert.AreEqual(
    //   Path.Combine(
    //     SettingsTestHelper.TemplatesFolderPath, "Strings", "Guitar Stream.uvip"), 
    //   category.TemplateProgramPath);
    Assert.IsNotNull(category.TemplateScriptProcessor);
    Assert.That(category.GetProgramFilesToEdit().Any());
  }

  [Test]
  public void Test2() {
    var category = new Category(GetSoundBankFolder("Factory"), "Brutal Bass 2.1");
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Brutal Bass 2.1", category.TemplateCategoryName);
    Assert.AreEqual("808 Line", category.TemplateProgramName);
  }

  [Test]
  public void Test3() {
    var category = new Category(GetSoundBankFolder("Factory"), "Bass-Sub");
    category.Initialise();
    Assert.IsFalse(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void Test4() {
    var category = new Category(GetSoundBankFolder("Factory"), "RetroWave 2.5");
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
    var category = new Category(GetSoundBankFolder("Spectre"), "Polysynth");
    category.Initialise();
    Assert.IsFalse(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Factory", category.TemplateSoundBankName);
    Assert.AreEqual("Keys", category.TemplateCategoryName);
    Assert.AreEqual("DX Mania", category.TemplateProgramName);
  }

  [Test]
  public void Test6() {
    var category = new Category(GetSoundBankFolder("Pulsar"), "Bass");
    category.Initialise();
    Assert.IsTrue(category.IsInfoPageLayoutInScript);
    Assert.AreEqual("Pulsar", category.TemplateSoundBankName);
    Assert.AreEqual("Bass", category.TemplateCategoryName);
    Assert.AreEqual("Warped", category.TemplateProgramName);
  }

  [Test]
  public void Test7() {
    var category = new Category(GetSoundBankFolder("Pulsar"), "Wrong");
    Assert.Throws<ApplicationException>(()=> category.Initialise());
  }

  [Test]
  public void Test8() {
    var category = new Category(GetSoundBankFolder("Xyz"), "Bass");
    Assert.Throws<ApplicationException>(()=> category.Initialise());
  }

  private static DirectoryInfo GetSoundBankFolder(string soundBankName) {
    var result = new DirectoryInfo(
      Path.Combine(
        SettingsTestHelper.ProgramsFolderPath, soundBankName));
    return result;
  }
}