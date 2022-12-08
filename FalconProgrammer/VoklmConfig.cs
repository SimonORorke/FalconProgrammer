namespace FalconProgrammer;

public class VoklmConfig : ScriptConfig {
  public VoklmConfig() : base(
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Programs\Voklm\Synth Choirs\Breath Five.uvip", 
    "EventProcessor0") { }

  public override void ConfigureMacroCcs(string soundBankName,
    string? categoryName = null) {
    InfoPageCcsScriptProcessorName = categoryName == "Vox Instruments"
      ? "EventProcessor9"
      : TemplateScriptProcessorName;
    base.ConfigureMacroCcs(soundBankName, categoryName);
  }
}