using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class OrganicPadsSettings {
  [XmlAttribute] public float AttackSeconds { get; set; }
  [XmlAttribute] public float ReleaseSeconds { get; set; }
  [XmlAttribute] public float MaxAttackSeconds { get; set; }
  [XmlAttribute] public float MaxDecaySeconds { get; set; }
  [XmlAttribute] public float MaxReleaseSeconds { get; set; }
}