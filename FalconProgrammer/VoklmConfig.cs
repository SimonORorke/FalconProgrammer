namespace FalconProgrammer; 

public class VoklmConfig : ScriptConfig {
  public VoklmConfig() : base(
    "Voklm", "Synth Choirs",
    "Breath Five", "EventProcessor0") { }

  public override void ConfigureMacroCcs(string programCategory) {
    MacroCcsScriptProcessorName = programCategory == "Vox Instruments"
      ? "EventProcessor9"
      : TemplateScriptProcessorName;
    base.ConfigureMacroCcs(programCategory);
  }
}