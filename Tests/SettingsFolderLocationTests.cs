namespace FalconProgrammer.Tests;

public class SettingsFolderLocationTests {
  public const string TestApplicationName = "TestFalconProgrammer";
  
  public static string SettingsFolderPath { get; } = Path.Combine(
    TestContext.CurrentContext.TestDirectory, TestApplicationName);

  [Test]
  public void Test1() {
    DeleteAnyTestData();
    try {
      var location1 = SettingsFolderLocation.Read(TestApplicationName);
      Assert.IsEmpty(location1.Path);
      location1.Path = SettingsFolderPath;
      location1.Write(TestApplicationName);
      Assert.That(()=> Directory.Exists(SettingsFolderPath));
      var location2 = SettingsFolderLocation.Read(TestApplicationName);
      Assert.AreEqual(SettingsFolderPath, location2.Path);
    } finally {
      DeleteAnyTestData();
    }
  }

  public static void DeleteAnyTestData() {
    if (Directory.Exists(SettingsFolderPath)) {
      Directory.Delete(SettingsFolderPath);
    }
    var locationFile = SettingsFolderLocation.GetSettingsFolderLocationFile(
      TestApplicationName);
    if (locationFile.Exists) {
      locationFile.Delete();
    }
    var appDataFolder = SettingsFolderLocation.GetAppDataFolder(
      TestApplicationName);
    if (appDataFolder.Exists) {
      appDataFolder.Delete();
    }
  }
}