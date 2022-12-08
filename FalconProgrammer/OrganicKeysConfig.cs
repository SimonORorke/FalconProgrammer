using FalconProgrammer.XmlLinq;

namespace FalconProgrammer;

public class OrganicKeysConfig : ScriptConfig {
  public OrganicKeysConfig() : base(
    @"D:\Simon\OneDrive\Documents\Music\Software\UVI Falcon\Programs\Organic Keys\Acoustic Mood\A Rhapsody.uvip", 
    "EventProcessor0") {
  }

  protected override ProgramXml CreateProgramXml() {
    return new OrganicKeysProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor!);
  }
}