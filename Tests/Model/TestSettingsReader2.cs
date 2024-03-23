using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class TestSettingsReader2 : SettingsReader {
  public string DefaultSettingsFolderPath { get; set; } = 
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI\FalconProgrammer.Data\Settings";

  protected override string GetDefaultSettingsFolderPath() {
    return DefaultSettingsFolderPath;
  }
}