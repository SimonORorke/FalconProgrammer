using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer.XmlLinq;

public class OrganicKeysProgramXml : ScriptProgramXml {
  public OrganicKeysProgramXml(
    Category category, ScriptProcessor infoPageCcsScriptProcessor) : base(
    category, infoPageCcsScriptProcessor) { }

  public override void UpdateInfoPageCcsScriptProcessor() {
    // Initialise Delay and Reverb to zero.
    const string delaySendAttributeName = "delaySend";
    var delaySendAttribute = 
      InfoPageCcsScriptProcessorElement!.Attribute(delaySendAttributeName) ??
      throw new ApplicationException(
        "Cannot find ScriptProcessor.delaySend attribute.");
    delaySendAttribute.Value = "0";
    const string reverbSendAttributeName = "reverbSend";
    var reverbSendAttribute = 
      InfoPageCcsScriptProcessorElement!.Attribute(reverbSendAttributeName) ??
      throw new ApplicationException(
        "Cannot find ScriptProcessor.reverbSend attribute.");
    reverbSendAttribute.Value = "0";
    base.UpdateInfoPageCcsScriptProcessor();
  }
}