namespace FalconProgrammer;

public static class Runner {
  public static void ConfigureCcs() {
    var config = new ProgramConfig();
    config.ConfigureMacroCcs("Devinity", "Plucks-Leads");
    //config.ConfigureMacroCcs("Devinity", "Bass XML");
    // var config = new ProgramConfig(
    //   "Factory", 
    //   "Keys XML WITH TEMPLATE", "DX Mania");
    // config.ConfigureMacroCcs("Keys XML");
    // var config = new OrganicKeysConfig();
    // config.ConfigureMacroCcs("Mono Chords XML");
    // var config = new Voklm();
    // config.ConfigureCcs("Synth Choirs");
    // config.ConfigureCcs("Vox Instruments");
    // var config = new OrganicKeys();
    // config.ConfigureCcs("Lo-Fi");
  }
}