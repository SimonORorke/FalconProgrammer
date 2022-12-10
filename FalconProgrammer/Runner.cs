namespace FalconProgrammer;

public static class Runner {
  public static void ConfigureCcs() {
    var config = new ProgramConfig();
    // config.ConfigureMacroCcs("Hypnotic Drive", "Leads");
    // var config = new ProgramConfig {
    //   MacroCcLocationOrder = LocationOrder.LeftToRightTopToBottom
    // };
    // config.ConfigureMacroCcs("Spectre");
    config.ConfigureMacroCcs("Factory","Organic Texture 2.8");
    // config.ConfigureMacroCcs("Hypnotic Drive","Synths");
    // config.ConfigureMacroCcs("Factory","Pure Additive 2.0");
    // config.ConfigureMacroCcs("Factory","Keys");
    // config.ConfigureMacroCcs("Devinity", "Plucks-Leads");
    //config.ConfigureMacroCcs("Devinity", "Bass XML");
    // var config = new ProgramConfig(
    //   "Factory", 
    //   "Keys XML WITH TEMPLATE", "DX Mania");
    // var config = new OrganicKeysConfig();
    // config.ConfigureMacroCcs("Mono Chords XML");
    // var config = new Voklm();
    // config.ConfigureCcs("Synth Choirs");
    // config.ConfigureCcs("Vox Instruments");
    // var config = new OrganicKeys();
    // config.ConfigureCcs("Lo-Fi");
  }
}