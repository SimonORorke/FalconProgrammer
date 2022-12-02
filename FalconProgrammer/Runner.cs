namespace FalconProgrammer;

public static class Runner {
  public static void ConfigureCcs() {
    var config = new ProgramConfig(
      "Factory", 
      "Keys WITH TEMPLATE XML", "DX Mania");
    config.ConfigureMacroCcs("Keys XML");
    // var config = new ProgramConfig(
    //   "Factory", 
    //   "Keys WITH TEMPLATE XML", "DX Mania");
    // config.ConfigureCcs("Keys XML");
    // var config = new Voklm();
    // config.ConfigureCcs("Synth Choirs");
    // config.ConfigureCcs("Vox Instruments");
    // var config = new OrganicKeys();
    // config.ConfigureCcs("Lo-Fi");
  }
}