using System.Xml.Linq;

namespace FalconProgrammer.XmlLinq;

public class OrganicKeysScriptProcessor : ScriptProcessor {
  public OrganicKeysScriptProcessor(XElement scriptProcessorElement,
    ProgramXml programXml) : base(scriptProcessorElement, programXml) {
    DelayReverb = 0;
    DelaySend = 0;
  }

  public float DelayReverb {
    get => Convert.ToSingle(
      GetAttributeValue(nameof(DelayReverb).ToLower()));
    set => SetAttribute(nameof(DelayReverb).ToLower(), value);
  }
  
  public float DelaySend {
    get => Convert.ToSingle(
      GetAttributeValue(nameof(DelaySend).ToLower()));
    set => SetAttribute(nameof(DelaySend).ToLower(), value);
  }
}