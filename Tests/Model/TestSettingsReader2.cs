using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestSettingsReader2 : SettingsReader {
  public string DefaultSettingsFolderPath { get; set; } = 
    SettingsTestHelper.DefaultSettingsFolderPath;

  protected override string GetDefaultSettingsFolderPath() {
    return DefaultSettingsFolderPath;
  }
}