using System.Diagnostics.CodeAnalysis;

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
      Assert.That(location1.Path, Is.Empty);
      location1.Path = SettingsFolderPath;
      location1.Write(TestApplicationName);
      Assert.That(Directory.Exists(SettingsFolderPath));
      var location2 = SettingsFolderLocation.Read(TestApplicationName);
      Assert.That(location2.Path, Is.EqualTo(SettingsFolderPath));
    } finally {
      DeleteAnyTestData();
    }
  }

  [SuppressMessage("Structure", "NUnit1028:The non-test method is public")]
  internal static void DeleteAnyTestData() {
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