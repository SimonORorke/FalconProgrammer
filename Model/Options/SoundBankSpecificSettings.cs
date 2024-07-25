using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class SoundBankSpecificSettings {
  public SoundBankSpecificSettings() {
    EtherFields.StandardLayout = true;
    Fluidity.MoveAttackMacroToEnd = true;
    OrganicPads.AttackSeconds = 0.02f;
    OrganicPads.ReleaseSeconds = 0.3f;
    OrganicPads.MaxAttackSeconds = 1f;
    OrganicPads.MaxDecaySeconds = 15f;
    OrganicPads.MaxReleaseSeconds = 2f;
    Spectre.StandardLayout = true;
  }

  [XmlElement]
  public StandardLayoutSetting EtherFields { get; set; } =
    new StandardLayoutSetting();

  [XmlElement] public FluiditySettings Fluidity { get; set; } = new FluiditySettings();

  [XmlElement]
  public OrganicPadsSettings OrganicPads { get; set; } =
    new OrganicPadsSettings();

  [XmlElement]
  public StandardLayoutSetting Spectre { get; set; } =
    new StandardLayoutSetting();
}