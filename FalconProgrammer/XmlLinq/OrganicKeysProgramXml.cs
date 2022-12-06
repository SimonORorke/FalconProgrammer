using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class OrganicKeysProgramXml : ScriptProgramXml {
  public OrganicKeysProgramXml(
    string templateProgramPath, ScriptProcessor infoPageCcsScriptProcessor) : base(
    templateProgramPath, infoPageCcsScriptProcessor) { }

  public override void UpdateInfoPageCcsScriptProcessor() {
    // Initialise Delay and Reverb to zero.
    const string delaySendAttributeName = "delaySend";
    var delaySendAttribute = 
      InfoPageCcsScriptProcessorElement!.Attribute(delaySendAttributeName) ??
      throw new ApplicationException(
        $"Cannot find {nameof(ScriptProcessor)}.{delaySendAttributeName} attribute.");
    delaySendAttribute.Value = "0";
    const string reverbSendAttributeName = "reverbSend";
    var reverbSendAttribute = 
      InfoPageCcsScriptProcessorElement!.Attribute(reverbSendAttributeName) ??
      throw new ApplicationException(
        $"Cannot find {nameof(ScriptProcessor)}.{reverbSendAttributeName} attribute.");
    reverbSendAttribute.Value = "0";
    base.UpdateInfoPageCcsScriptProcessor();
  }
}