namespace FalconProgrammer;

public static class Utility {
  public const string ProgramExtension = ".uvip";
  public const string SynthName = "UVI Falcon";

  public static void ConfigureCcs() {
    var config = new Voklm();
    config.ConfigureCcs("Synth Choirs");
    config.ConfigureCcs("Vox Instruments");
    // var config = new OrganicKeys();
    // config.ConfigureCcs("Lo-Fi");
  }
}