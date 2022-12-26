﻿using System.Drawing;
using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class FalconProgram {
  public FalconProgram(string path, Category category) {
    Path = path;
    Category = category;
  }

  [PublicAPI] public Category Category { get; }
  public List<ConstantModulation> ConstantModulations { get; set; } = null!;
  public ScriptProcessor? InfoPageCcsScriptProcessor { get; private set; }
  public string Name { get; private set; } = null!;
  private int NextContinuousCcNo { get; set; } = 31;
  private int NextToggleCcNo { get; set; } = 112;
  public string Path { get; }
  private List<ScriptProcessor> ScriptProcessors { get; set; } = null!;

  private static void CheckForNonModWheelNonInfoPageMacro(
    SignalConnection signalConnection) {
    if (!signalConnection.IsForInfoPageMacro
        // ReSharper disable once MergeIntoPattern
        && signalConnection.CcNo.HasValue && signalConnection.CcNo != 1) {
      throw new ApplicationException(
        $"MIDI CC {signalConnection.CcNo} is mapped to " +
        $"{signalConnection.Destination}, which is not a Info page macro.");
    }
  }

  /// <summary>
  ///   Modulation wheel (MIDI CC 1) is the only MIDI CC number expected not to control
  ///   a macro on the Info page. If there are others, there is a risk that they could
  ///   duplicate CC numbers we map to Info page macros.  So let's validate that there
  ///   are none.
  /// </summary>
  private void CheckForNonModWheelNonInfoPageMacros() {
    foreach (
      var signalConnection in ConstantModulations.SelectMany(constantModulation =>
        constantModulation.SignalConnections)) {
      CheckForNonModWheelNonInfoPageMacro(signalConnection);
    }
    foreach (
      var signalConnection in ScriptProcessors.SelectMany(scriptProcessor =>
        scriptProcessor.SignalConnections)) {
      CheckForNonModWheelNonInfoPageMacro(signalConnection);
    }
  }

  public void Deserialise() {
    using var reader = new StreamReader(Path);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    Name = root.Program.DisplayName;
    ConstantModulations = root.Program.ConstantModulations;
    ScriptProcessors = root.Program.ScriptProcessors;
    InfoPageCcsScriptProcessor = FindInfoPageCcsScriptProcessor();
    CheckForNonModWheelNonInfoPageMacros();
  }

  private ScriptProcessor? FindInfoPageCcsScriptProcessor() {
    // When the Info page layout is defined in a script, it may be fine to simplify this
    // and always pick the last ScriptProcessor. So I will try not bothering to specify
    // any more non-standard ScriptProcessor names. 
    //
    // Sometimes the Info page layout ScriptProcessor name is not consistent across all
    // programs in this category. E.g. for "Factory\Organic Texture 2.8\BEL SoToy" it's
    // "EventProcessor1", while for programs alphabetically prior to that one it's
    // "EventProcessor0". So, if there's only one ScriptProcessor in the program,
    // it must be the right one!
    if (Category.IsInfoPageLayoutInScript &&
        ScriptProcessors.Count == 1) {
      return ScriptProcessors[0];
    }
    // If there's two or more script processors, we need to know the name of the
    // Info page layout ScriptProcessor.
    var withName = (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == Category.InfoPageCcsScriptProcessorName
      select scriptProcessor).FirstOrDefault();
    if (withName != null || !Category.IsInfoPageLayoutInScript) {
      return withName;
    }
    // When there's no ScriptProcessor with the designated name yet we expect the
    // Info page layout to be defined in a script, guess the last ScriptProcessor. That
    // works for "Factory\RetroWave 2.5\BAS Endless Droids"
    return ScriptProcessors.Any() ? ScriptProcessors[^1] : null;
  }

  public int GetCcNo(ConstantModulation constantModulation) {
    int result;
    if (constantModulation.IsContinuous) {
      // Map continuous controller CC to continuous macro. 
      // Convert the fifth continuous controller's CC number to 11 to map to the touch
      // strip.
      result = NextContinuousCcNo != 35 ? NextContinuousCcNo : 11;
      NextContinuousCcNo++;
    } else {
      // Map button CC to toggle macro. 
      result = NextToggleCcNo;
      NextToggleCcNo++;
    }
    return result;
  }

  public List<ConstantModulation> GetContinuousMacros() {
    return (
      from macro in ConstantModulations
      where macro.IsContinuous
      select macro).ToList();
  }

  public Point? GetLocationForNewMacro() {
    const int macroWidth = 60;
    const int minHorizontalGapBetweenMacros = 5;
    const int minNewMacroGapWidth = macroWidth + 2 * minHorizontalGapBetweenMacros;
    const int rightEdge = 675;
    int bottomRowY = (
      from macro in ConstantModulations
      select macro.Properties.Y).Max();
    var bottomRowMacros = (
      from macro in GetMacrosSortedByLocation(
        LocationOrder.TopToBottomLeftToRight)
      where macro.Properties.Y == bottomRowY
      select macro).ToList();
    var gapWidths = new List<int> { bottomRowMacros[0].Properties.X };
    if (bottomRowMacros.Count > 1) {
      for (int i = 0; i < bottomRowMacros.Count - 1; i++) {
        gapWidths.Add(
          bottomRowMacros[i + 1].Properties.X
          - (bottomRowMacros[i].Properties.X
          + macroWidth));
      }
    }
    gapWidths.Add(rightEdge - (bottomRowMacros[^1].Properties.X + macroWidth));
    int maxGapWidth = (from gapWidth in gapWidths select gapWidth).Max();
    if (maxGapWidth < minNewMacroGapWidth) {
      return null;
    }
    int newMacroGapIndex = -1;
    for (int i = 0; i < gapWidths.Count; i++) {
      if (gapWidths[i] == maxGapWidth) {
        newMacroGapIndex = i;
        break;
      }
    }
    int newMacroGapX = newMacroGapIndex == 0
      ? 0
      : bottomRowMacros[newMacroGapIndex - 1].Properties.X + macroWidth;
    int newMacroX = newMacroGapX + (maxGapWidth - macroWidth) / 2;
    return new Point(newMacroX, bottomRowY);
  }

  public SortedSet<ConstantModulation> GetMacrosSortedByLocation(
    LocationOrder macroCcLocationOrder) {
    var result = new SortedSet<ConstantModulation>(
      macroCcLocationOrder == LocationOrder.TopToBottomLeftToRight
        ? new TopToBottomLeftToRightComparer()
        : new LeftToRightTopToBottomComparer());
    for (int i = 0; i < ConstantModulations.Count; i++) {
      var macro = ConstantModulations[i];
      // This validation is not reliable. In "Factory\Bells\Glowing 1.2", the macros with
      // ConstantModulation.Properties showValue="0" are shown on the Info page. 
      //constantModulation.Properties.Validate();
      macro.Index = i;
      for (int j = 0; j < macro.SignalConnections.Count; j++) {
        var signalConnection = macro.SignalConnections[j];
        signalConnection.Index = j;
      }
      // In the Devinity sound bank, some macros do not appear on the Info page (only
      // the Mods page). For example Devinity/Bass/Comber Bass.
      // This is achieved by setting, in ConstantModulation.Properties, the X coordinates
      // of all those macros to 999, presumably off the right edge of the Info page, and
      // the Y coordinates to 353.
      // (Those ConstantModulation.Properties do not have the optional attribute
      // showValue="0".)
      // I don't know whether that is standard practice or just a trick in Devinity.
      // So, to prevent CC numbers from being given to macros that do not appear on the
      // Info page, omit all macros with duplicate locations from this set. Those macros
      // do not need CC numbers, and attempting to add duplicates to the set would throw
      // an exception in ConstantModulationLocationComparer.
      if (HasUniqueLocation(macro)) {
        result.Add(macro);
      }
    }
    return result;
  }

  private bool HasUniqueLocation(ConstantModulation macro) {
    return (
      from m in ConstantModulations
      where m.Properties.X == macro.Properties.X
            && m.Properties.Y == macro.Properties.Y
      select m).Count() == 1;
  }
}