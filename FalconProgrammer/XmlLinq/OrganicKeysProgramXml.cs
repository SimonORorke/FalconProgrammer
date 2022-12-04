using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class OrganicKeysProgramXml : ProgramXml {
  public OrganicKeysProgramXml(string templatePath, ProgramConfig programConfig) : base(
    templatePath, programConfig) { }

  public override void UpdateMacroCcsScriptProcessor() {
    // Initialise Delay and Reverb to zero.
    const string delaySendAttributeName = "delaySend";
    var delaySendAttribute = 
      MacroCcsScriptProcessorElement!.Attribute(delaySendAttributeName) ??
      throw new ApplicationException(
        $"Cannot find {nameof(ScriptProcessor)}.{delaySendAttributeName} attribute.");
    delaySendAttribute.Value = "0";
    const string reverbSendAttributeName = "reverbSend";
    var reverbSendAttribute = 
      MacroCcsScriptProcessorElement!.Attribute(reverbSendAttributeName) ??
      throw new ApplicationException(
        $"Cannot find {nameof(ScriptProcessor)}.{reverbSendAttributeName} attribute.");
    reverbSendAttribute.Value = "0";
    base.UpdateMacroCcsScriptProcessor();
  }
}