using FalconProgrammer.XmlLinq;

namespace FalconProgrammer;

public class OrganicKeysConfig : ScriptConfig {
  public OrganicKeysConfig() : base(
    "Organic Keys", "Acoustic Mood XML",
    "A Rhapsody", "EventProcessor0") {
  }

  protected override ProgramXml CreateProgramXml() {
    return new OrganicKeysProgramXml(TemplateProgramPath, this);
  }
}