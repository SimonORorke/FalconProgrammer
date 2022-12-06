namespace FalconProgrammer; 

public class VoklmConfig : ScriptConfig {
  public VoklmConfig() : base(
    "Voklm", "Synth Choirs",
    "Breath Five", "EventProcessor0") { }

  public override void ConfigureMacroCcs(string soundBankName, string categoryName) {
    InfoPageCcsScriptProcessorName = categoryName == "Vox Instruments"
      ? "EventProcessor9"
      : TemplateScriptProcessorName;
    base.ConfigureMacroCcs(soundBankName, categoryName);
  }
}