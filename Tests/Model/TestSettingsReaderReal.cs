using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

/// <summary>
///   A test Settings reader that reads real files. 
/// </summary>
public class TestSettingsReaderReal : SettingsReader {
  public string DefaultSettingsFolderPath { get; set; } = 
    SettingsTestHelper.DefaultSettingsFolderPath;

  protected override string GetDefaultSettingsFolderPath() {
    return DefaultSettingsFolderPath;
  }
}