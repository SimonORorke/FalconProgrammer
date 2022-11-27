namespace FalconProgrammer;

public static class Utility {
  public const string ProgramExtension = ".uvip";
  public const string SynthName = "UVI Falcon";

  public static void ConfigureCcs() {
    var config = new OrganicKeysConfig();
    config.ConfigureCcs("Lo-Fi");
  }
}