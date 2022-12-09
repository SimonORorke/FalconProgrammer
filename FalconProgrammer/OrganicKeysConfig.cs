using FalconProgrammer.XmlLinq;

namespace FalconProgrammer;

public class OrganicKeysConfig : ScriptConfig {
  protected override ProgramXml CreateProgramXml() {
    return new OrganicKeysProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor!);
  }
}