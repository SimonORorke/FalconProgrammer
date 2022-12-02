namespace FalconProgrammer; 

public class Voklm : ScriptConfig {
  public Voklm() : base(
    "Voklm", "Synth Choirs",
    "Breath Five", "EventProcessor0") { }

  public override void ConfigureMacroCcs(string programCategory) {
    ScriptProcessorName = programCategory == "Vox Instruments"
      ? "EventProcessor9"
      : TemplateScriptProcessorName;
    base.ConfigureMacroCcs(programCategory);
  }
}