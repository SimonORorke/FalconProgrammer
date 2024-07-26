using System.Collections.Immutable;
using System.Xml.Serialization;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Model.Options;

public class MidiForMacros {
  private ImmutableList<int>? _continuousCcNos;
  private ImmutableList<int>? _toggleCcNos;
  [XmlAttribute] public int ModWheelReplacementCcNo { get; set; }

  /// <summary>
  ///   Gets or sets whether to append the MIDI CC number of each <see cref="Macro" />'s
  ///   non-mod wheel <see cref="Modulation" /> to the macro's
  ///   <see cref="EntityBase.DisplayName " /> when updating the MIDI Ccs assigned to
  ///   macros of programs that do not have script-defined GUIs.
  /// </summary>
  [XmlAttribute]
  public bool AppendCcNoToMacroDisplayNames { get; set; } = true;

  [XmlArray(nameof(ContinuousCcNoRanges))]
  [XmlArrayItem("ContinuousCcNoRange")]
  public List<IntegerRange> ContinuousCcNoRanges { get; set; } = [];

  [XmlArray(nameof(ToggleCcNoRanges))]
  [XmlArrayItem("ToggleCcNoRange")]
  public List<IntegerRange> ToggleCcNoRanges { get; set; } = [];

  [XmlArray(nameof(DoNotReplaceModWheelWithMacroSoundBanks))]
  [XmlArrayItem("SoundBankName")]
  public List<string> DoNotReplaceModWheelWithMacroSoundBanks { get; set; } = [];

  internal ImmutableList<int> ContinuousCcNos =>
    _continuousCcNos ??= CreateCcNoList(ContinuousCcNoRanges);

  internal int CurrentContinuousCcNo { get; set; }
  internal int CurrentToggleCcNo { get; set; }
  internal bool HasModWheelReplacementCcNo => ModWheelReplacementCcNo > 1;

  internal ImmutableList<int> ToggleCcNos =>
    _toggleCcNos ??= CreateCcNoList(ToggleCcNoRanges);

  internal bool CanReplaceModWheelWithMacro(string soundBankName) {
    return !DoNotReplaceModWheelWithMacroSoundBanks.Contains(soundBankName);
  }

  private static ImmutableList<int> CreateCcNoList(List<IntegerRange> ranges) {
    var list = new List<int>();
    foreach (var range in ranges) {
      for (int ccNo = range.Start; ccNo <= range.End; ccNo++) {
        list.Add(ccNo);
      }
    }
    return list.ToImmutableList();
  }

  private static int GetCcNoAfter(int prevCcNo, ImmutableList<int> ccNos) {
    if (prevCcNo == 0) {
      return ccNos[0];
    }
    int prevIndex = ccNos.IndexOf(prevCcNo);
    if (prevIndex >= 0 && prevIndex <= ccNos.Count - 2) {
      // There's at least 1 MIDI CC number after the previous MIDI CC number in the list
      // of MIDI CC numbers.
      return ccNos[prevIndex + 1];
    }
    return prevCcNo + 1;
  }

  private int GetContinuousCcNoAfter(bool reuseCc1) {
    if (HasModWheelReplacementCcNo) {
      if (CurrentContinuousCcNo == 1) {
        return ContinuousCcNos[
          ContinuousCcNos.IndexOf(ModWheelReplacementCcNo) + 1];
      }
      if (CurrentContinuousCcNo == ModWheelReplacementCcNo && reuseCc1) {
        return 1; // Wheel
      }
    }
    return GetCcNoAfter(CurrentContinuousCcNo, ContinuousCcNos);
  }

  public int GetNextContinuousCcNo(bool reuseCc1) {
    CurrentContinuousCcNo = GetContinuousCcNoAfter(reuseCc1);
    return CurrentContinuousCcNo;
  }

  public int GetNextToggleCcNo() {
    CurrentToggleCcNo = GetCcNoAfter(CurrentToggleCcNo, ToggleCcNos);
    return CurrentToggleCcNo;
  }
}