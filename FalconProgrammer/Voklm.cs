namespace FalconProgrammer; 

public class Voklm : ScriptConfig {
  public Voklm() : base(
    "Voklm", "Synth Choirs",
    "Breath Five", "EventProcessor0") { }

  public override void ConfigureCcs(string programCategory) {
    ScriptProcessorName = programCategory == "Vox Instruments"
      ? "EventProcessor9"
      : TemplateScriptProcessorName;
    base.ConfigureCcs(programCategory);
  }
}