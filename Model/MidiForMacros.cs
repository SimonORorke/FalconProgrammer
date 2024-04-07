using System.Collections.Immutable;
using System.Xml.Serialization;

namespace FalconProgrammer.Model;

public class MidiForMacros {
  private ImmutableList<int>? _continuousCcNos;
  private ImmutableList<int>? _toggleCcNos;
  [XmlAttribute] public int ModWheelReplacementCcNo { get; set; }

  [XmlArray(nameof(ContinuousCcNoRanges))]
  [XmlArrayItem("ContinuousCcNoRange")]
  public List<Settings.IntegerRange> ContinuousCcNoRanges { get; set; } = [];

  [XmlArray(nameof(ToggleCcNoRanges))]
  [XmlArrayItem("ToggleCcNoRange")]
  public List<Settings.IntegerRange> ToggleCcNoRanges { get; set; } = [];

  internal ImmutableList<int> ContinuousCcNos =>
    _continuousCcNos ??= CreateCcNoList(ContinuousCcNoRanges);

  internal ImmutableList<int> ToggleCcNos =>
    _toggleCcNos ??= CreateCcNoList(ToggleCcNoRanges);

  private static ImmutableList<int> CreateCcNoList(List<Settings.IntegerRange> ranges) {
    var list = new List<int>();
    foreach (var range in ranges) {
      for (int ccNo = range.Start; ccNo <= range.End; ccNo++) {
        list.Add(ccNo);
      }
    }
    return list.ToImmutableList();
  }
}