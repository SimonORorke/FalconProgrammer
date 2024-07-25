using System.Xml.Serialization;

namespace FalconProgrammer.Model.Options;

public class FluiditySettings {
  [XmlAttribute] public bool MoveAttackMacroToEnd { get; set; }
}