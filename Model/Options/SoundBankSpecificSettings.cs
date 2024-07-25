using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class SoundBankSpecificSettings {
  [XmlElement] public StandardLayoutSetting EtherFields { get; set; } =
    new StandardLayoutSetting();
    
  [XmlElement] public FluiditySettings Fluidity { get; set; } = new FluiditySettings();

  [XmlElement]
  public OrganicPadsSettings OrganicPads { get; set; } =
    new OrganicPadsSettings();
    
  [XmlElement] public StandardLayoutSetting Spectre { get; set; } =
    new StandardLayoutSetting();
}