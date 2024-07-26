using System.Xml.Serialization;

namespace FalconProgrammer.Model.Mpe;

public class MpeSettings {
  public MpeSettings() {
    YTargetValue = Mpe.YTarget.ContinuousMacro1Bipolar;
    ZTargetValue = Mpe.ZTarget.ContinuousMacro2Unipolar;
    XTargetValue = Mpe.XTarget.Pitch;
    GainMapValue = Mpe.GainMap.TwentyDb;
  }

  [XmlAttribute] public string YTarget { get; set; } = null!;
  [XmlAttribute] public string ZTarget { get; set; } = null!;
  [XmlAttribute] public string XTarget { get; set; } = null!;
  [XmlAttribute] public string GainMap { get; set; } = null!;
  [XmlAttribute] public bool InitialiseZToMacroValue { get; set; }

  internal YTarget YTargetValue {
    get => Global.GetEnumValue<YTarget>(YTarget);
    private set => YTarget = value.ToString();
  }

  internal ZTarget ZTargetValue {
    get => Global.GetEnumValue<ZTarget>(ZTarget);
    private set => ZTarget = value.ToString();
  }

  internal XTarget XTargetValue {
    get => Global.GetEnumValue<XTarget>(XTarget);
    private set => XTarget = value.ToString();
  }

  public GainMap GainMapValue {
    get => Global.GetEnumValue<GainMap>(GainMap);
    set => GainMap = value.ToString();
  }
}