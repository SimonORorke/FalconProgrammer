﻿using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer;

public class InfoPageLayout {
  public InfoPageLayout(FalconProgram program) {
    Program = program;
  }

  private List<Macro> BottomRowMacros { get; set; } = null!;
  private int BottomRowY { get; set; }
  private int MaxExistingContinuousMacroCount { get; set; }
  private int ModWheelReplacementCcNo { get; set; }
  private FalconProgram Program { get; }

  /// <summary>
  ///   If feasible, replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro. Otherwise shows a message
  ///   explaining why it is not feasible.
  /// </summary>
  public void ReplaceModWheelWithMacro(
    int modWheelReplacementCcNo, int maxExistingContinuousMacroCount) {
    ModWheelReplacementCcNo = modWheelReplacementCcNo;
    MaxExistingContinuousMacroCount = maxExistingContinuousMacroCount;
    Console.WriteLine($"Checking '{Program.Path}'.");
    if (Program.InfoPageCcsScriptProcessor != null) {
      Console.WriteLine(
        $"'{Program.Name}': Replacing wheel with macro is not supported because " +
        "there is an Info page CCs script processor.");
      return;
    }
    if (WheelMacroExists()) {
      return;
    }
    if (!Program.ProgramXml.HasModWheelSignalConnections()) {
      Console.WriteLine($"'{Program.Name}' contains no mod wheel modulations.");
      return;
    }
    var locationForNewWheelMacro = FindLocationForNewWheelMacro(
      out bool updateMacroCcs);
    if (locationForNewWheelMacro == null) {
      return;
    }
    AddWheelMacro(locationForNewWheelMacro.Value, updateMacroCcs);
  }

  private void AddWheelMacro(Point location, bool updateMacroCcs) {
    int wheelMacroNo = (
      from macro in Program.Macros
      select macro.MacroNo).Max() + 1;
    var wheelMacro = new Macro {
      MacroNo = wheelMacroNo,
      DisplayName = "Wheel",
      Bipolar = 0,
      IsContinuous = true,
      Value = 0,
      SignalConnections = new List<SignalConnection> {
        new SignalConnection {
          CcNo = ModWheelReplacementCcNo
        }
      },
      Properties = new Properties {
        X = location.X,
        Y = location.Y
      }
    };
    Program.Macros.Add(wheelMacro);
    Program.ProgramXml.AddMacro(wheelMacro);
    Program.ProgramXml.ChangeModWheelSignalConnectionSourcesToMacro(wheelMacro);
    if (updateMacroCcs) {
      Program.UpdateMacroCcs(LocationOrder.LeftToRightTopToBottom);
    }
    Console.WriteLine(
      $"'{Program.Name}': Replaced mod wheel with macro.");
  }

  private Macro? FindContinuousMacroWithModWheelReplacementCcNo() {
    return (
      from continuousMacro in Program.ContinuousMacros
      let maybeSignalConnection =
        (from signalConnection in continuousMacro.SignalConnections
          where signalConnection.CcNo == ModWheelReplacementCcNo
          select signalConnection).FirstOrDefault()
      where maybeSignalConnection != null
      select continuousMacro).FirstOrDefault();
  }

  private Macro? FindDelayContinuousMacro() {
    return (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.ControlsDelay
      select continuousMacro).FirstOrDefault();
  }

  private void FindDelayOrReverbMacroWithModWheelReplacementCcNo(
    out Macro? delayOrReverbMacroWithWheelCcNo,
    out SignalConnection? delayOrReverbSignalConnectionWithWheelCcNo) {
    delayOrReverbMacroWithWheelCcNo = null;
    delayOrReverbSignalConnectionWithWheelCcNo = null;
    var continuousMacro = FindContinuousMacroWithModWheelReplacementCcNo();
    if (continuousMacro != null &&
        (continuousMacro.ControlsDelay || continuousMacro.ControlsReverb)) {
      delayOrReverbMacroWithWheelCcNo = continuousMacro;
      delayOrReverbSignalConnectionWithWheelCcNo = continuousMacro.SignalConnections[0];
    }
  }

  private Macro? FindDelayToggleMacro() {
    return (
      from macro in Program.Macros
      where macro.ControlsDelay && !macro.IsContinuous
      select macro).FirstOrDefault();
  }

  private Point? FindLocationForNewWheelMacro(out bool updateMacroCcs) {
    updateMacroCcs = false;
    BottomRowY = (
      from macro in Program.Macros
      select macro.Properties.Y).Max();
    BottomRowMacros = GetBottomRowMacros(BottomRowY);
    var result = LocateWheelAboveDelayOrReverbMacro();
    if (result != null) {
      return result;
    }
    // There is no wheel replacement CC number on a delay or reverb macro to reassign.
    result = LocateWheelMacroOnOrAboveBottomRow();
    if (result != null) {
      return result;
    }
    // There is no wheel replacement CC number on a delay or reverb macro to reassign
    // and there are more than {MaxExistingContinuousMacroCount} continuous macros.
    result = LocateWheelMacroAboveMacroWithWheelCcNo();
    if (result != null) {
      updateMacroCcs = true;
    }
    return result;
  }

  private Macro? FindReverbContinuousMacroWithWheelReplacementCcNo() {
    var macro = FindContinuousMacroWithModWheelReplacementCcNo();
    return macro is { ControlsReverb: true } ? macro : null;
  }

  private Macro? FindReverbToggleMacro() {
    return (
      from macro in Program.Macros
      where macro.ControlsReverb && !macro.IsContinuous
      select macro).FirstOrDefault();
  }

  private Macro? FindWheelMacro() {
    return (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.DisplayName.ToLower().Contains("wheel")
      select continuousMacro).FirstOrDefault();
  }

  /// <summary>
  ///   Returns, from left to right, the macros on the bottom row of macros on the Info
  ///   page.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  private List<Macro> GetBottomRowMacros(int bottomRowY) {
    // We need to horizontally align the new macro relative not only to macros that are
    // bottommost on the Info window (i.e. highest Y) but also those that are close to
    // the bottom.  The vertical clearance is 95, so this should be safe. In reality,
    // many are up just 5 from the bottommost macros.
    // Example: "Factory\Pluck\Mutan Mute".
    const int verticalFudge = 50;
    return (
      from macro in Program.GetMacrosSortedByLocation(
        LocationOrder.TopToBottomLeftToRight)
      where macro.Properties.Y >= bottomRowY - verticalFudge
      select macro).ToList();
  }

  private static Point LocateNewMacroAboveMacro(Macro macro) {
    // Allows a gap above a macro whose display name wraps to two text lines.
    // 95 would be the bare minimum.
    const int verticalClearance = 115;
    return new Point(
      macro.Properties.X,
      macro.Properties.Y - verticalClearance);
  }

  private Point? LocateWheelAboveDelayOrReverbMacro() {
    SwapDelayAndReverbIfReverbHasWheelReplacementCcNo();
    FindDelayOrReverbMacroWithModWheelReplacementCcNo(
      out var delayOrReverbMacroWithWheelCcNo,
      out var delayOrReverbSignalConnectionWithWheelCcNo);
    bool isDelayOrReverbMacroWithWheelCcNoOnBottomRow =
      delayOrReverbMacroWithWheelCcNo != null
      && BottomRowMacros.Contains(delayOrReverbMacroWithWheelCcNo);
    if (isDelayOrReverbMacroWithWheelCcNoOnBottomRow) {
      // Remove the wheel replacement CC number assignment from the delay or reverb
      // macro that has it.  It will be reassigned to the new wheel macro when that is
      // added.
      RemoveSignalConnection(
        delayOrReverbMacroWithWheelCcNo!, delayOrReverbSignalConnectionWithWheelCcNo!);
      // Locate the new wheel macro above the delay or reverb macro.
      return LocateNewMacroAboveMacro(delayOrReverbMacroWithWheelCcNo!);
    }
    return null;
  }

  private Point? LocateWheelMacroAboveMacroWithWheelCcNo() {
    var macro = FindContinuousMacroWithModWheelReplacementCcNo();
    if (macro == null) {
      return null;
    }
    return LocateNewMacroAboveMacro(macro);
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Point? LocateWheelMacroOnOrAboveBottomRow() {
    if (Program.ContinuousMacros.Count > MaxExistingContinuousMacroCount) {
      return null;
    }
    const int macroWidth = 60;
    const int minHorizontalGapBetweenMacros = 5;
    const int minNewMacroGapWidth = macroWidth + 2 * minHorizontalGapBetweenMacros;
    // When there are only toggle macros on the bottom row, they may be lower than the
    // standard bottom, usually to accomodate two-line display names.  This looks OK for
    // toggle macros.  But for continuous macros, being taller, it makes an ugly lack of
    // bottom margin.  So place the new continuous macro no lower than the standard
    // bottommost Y.
    // Example: "Factory\Pluck\Pad Mullerizer".
    const int standardBottommostY = 355;
    const int rightEdge = 695;
    // List, from left to right, the widths of the gaps between the macros on the bottom
    // row of macros on the Info page.  Include the gap between the leftmost macro and
    // the left edge and the gap between the rightmost macro and the right edge.
    var gapWidths = new List<int> { BottomRowMacros[0].Properties.X };
    if (BottomRowMacros.Count > 1) {
      for (int i = 0; i < BottomRowMacros.Count - 1; i++) {
        gapWidths.Add(
          BottomRowMacros[i + 1].Properties.X
          - (BottomRowMacros[i].Properties.X
             + macroWidth));
      }
    }
    gapWidths.Add(rightEdge - (BottomRowMacros[^1].Properties.X + macroWidth));
    // Check whether there any gaps on the bottom rowe wide enough to accommodate a new
    // macro.
    bool canFitInGap = (
      from gapWidth in gapWidths
      where gapWidth >= minNewMacroGapWidth
      select gapWidth).Any();
    if (!canFitInGap) {
      bool hasSingleRow = BottomRowMacros.Count == Program.Macros.Count;
      if (!hasSingleRow) {
        // No instances of this have been reported.
        Console.WriteLine(
          $"'{Program.Name}' " +
          "does not have room on its Info page's bottom row for a new macro.");
        return null;
      }
      // Locate the new wheel macro above the rightmost macro.
      // Example: Factory/Bass-Sub/Gamma Bass 1.4.
      var rightmostMacro = BottomRowMacros[^1];
      return LocateNewMacroAboveMacro(rightmostMacro);
    }
    // There is at least one gap wide enough to accommodate a new macro.
    // Put the new macro on the bottom row of macros, in the middle of the rightmost gap
    // within which it will fit.
    int rightmostSuitableGapWidth = (
      from gapWidth in gapWidths
      where gapWidth >= minNewMacroGapWidth
      select gapWidth).Last();
    int newMacroGapIndex = -1;
    for (int i = gapWidths.Count - 1; i >= 0; i--) {
      if (gapWidths[i] == rightmostSuitableGapWidth) {
        newMacroGapIndex = i;
        break;
      }
    }
    int newMacroGapX = newMacroGapIndex == 0
      ? 0
      : BottomRowMacros[newMacroGapIndex - 1].Properties.X + macroWidth;
    int newMacroX = newMacroGapX + (rightmostSuitableGapWidth - macroWidth) / 2;
    // If there are continuous and toggle macros on the bottom row, the continuous macros
    // may be a little higher up than the toggle macros, as they are taller.  In that
    // case, align the new macro horizontally with the bottommost continuous macro.
    // Example: "Factory\Pluck\Mutan Mute".
    var bottomRowContinuousMacros = (
      from macro in BottomRowMacros
      where macro.IsContinuous
      select macro).ToList();
    int newMacroY;
    if (bottomRowContinuousMacros.Count > 0) {
      newMacroY = (
        from macro in bottomRowContinuousMacros
        select macro.Properties.Y).Max();
    } else {
      newMacroY = BottomRowY <= standardBottommostY ? BottomRowY : standardBottommostY;
    }
    return new Point(newMacroX, newMacroY);
  }

  private void RemoveSignalConnection(Macro macro, SignalConnection signalConnection) {
    if (macro.SignalConnections.Contains(signalConnection)) {
      macro.SignalConnections.Remove(signalConnection);
    }
    Program.ProgramXml.RemoveSignalConnectionElementsWithSource(
      signalConnection.Source);
  }

  /// <summary>
  ///   If a reverb continuous macro has the MIDI CC number that is to be used for the
  ///   modulation wheel replacement continuous macro, if there is also a delay
  ///   continuous macro, swap the locations and CC numbers of the delay continuous macro
  ///   and the reverb continuous macro.  When and if that has been done, swap the
  ///   locations and CC numbers of the delay toggle macro and the reverb toggle macro,
  ///   if both of those exist.
  /// </summary>
  /// <remarks>
  ///   Any delay or reverb continuous macro that has the MIDI CC number that is to be
  ///   used for the modulation wheel replacement continuous macro will then have no MIDI
  ///   control.  As reverb tends to be more useful than delay, we are putting the delay
  ///   continuous macro into that position in advance, where applicable.
  ///   Example: Devinity/Bass/Tack Bass.
  /// </remarks>
  private void SwapDelayAndReverbIfReverbHasWheelReplacementCcNo() {
    var reverbContinuousMacro = FindReverbContinuousMacroWithWheelReplacementCcNo();
    if (reverbContinuousMacro != null
        && BottomRowMacros.Contains(reverbContinuousMacro)) {
      var delayContinuousMacro = FindDelayContinuousMacro();
      if (delayContinuousMacro != null) {
        SwapMacroLocations(delayContinuousMacro, reverbContinuousMacro);
        var reverbToggleMacro = FindReverbToggleMacro();
        if (reverbToggleMacro != null) {
          var delayToggleMacro = FindDelayToggleMacro();
          if (delayToggleMacro != null) {
            SwapMacroLocations(delayToggleMacro, reverbToggleMacro);
          }
        }
      }
    }
  }

  private void SwapMacroCcNos(Macro macro1, Macro macro2) {
    // ReSharper disable once ConvertIfStatementToSwitchStatement
    if (macro1.SignalConnections.Count == 0 && macro2.SignalConnections.Count == 0) {
      // This happens if the signal connections belongs to effects, a scenario we don't
      // (yet) support. Example: 'Factory/Pads/Lush Chords 2.0'.
      Console.WriteLine(
        $"'{Program.Name}': Cannot find SignalConnections of supported types for " +
        $"either macro '{macro1.DisplayName}' or macro '{macro2.DisplayName}'.");
    }
    // SignalConnections belong to Macros.
    SignalConnection? signalConnection1 = null;
    SignalConnection? signalConnection2 = null;
    if (macro1.SignalConnections.Count > 0) {
      signalConnection1 = macro1.SignalConnections[0];
      macro1.SignalConnections.Remove(signalConnection1);
      Program.ProgramXml.RemoveSignalConnectionElementsWithSource(
        signalConnection1.Source);
    }
    if (macro2.SignalConnections.Count > 0) {
      signalConnection2 = macro2.SignalConnections[0];
      macro2.SignalConnections.Remove(signalConnection2);
      Program.ProgramXml.RemoveSignalConnectionElementsWithSource(
        signalConnection2.Source);
    }
    if (signalConnection1 != null) {
      macro2.SignalConnections.Add(signalConnection1);
      Program.ProgramXml.AddMacroSignalConnection(signalConnection1, macro2);
    }
    if (signalConnection2 != null) {
      macro1.SignalConnections.Add(signalConnection2);
      Program.ProgramXml.AddMacroSignalConnection(signalConnection2, macro1);
    }
  }

  private void SwapMacroLocations(Macro macro1, Macro macro2) {
    var properties1 = macro1.Properties;
    var properties2 = macro2.Properties;
    macro1.Properties = properties2;
    macro2.Properties = properties1;
    Program.ProgramXml.UpdateMacroLocation(macro1);
    Program.ProgramXml.UpdateMacroLocation(macro2);
    // We also need to swap the MIDI CC numbers so that those still increment in the layout
    // order.
    SwapMacroCcNos(macro1, macro2);
  }

  private bool WheelMacroExists() {
    var wheelMacro = FindWheelMacro();
    if (wheelMacro != null) {
      Console.WriteLine(
        $"'{Program.Name}' already has a '{wheelMacro.DisplayName}' macro.");
      return true;
    }
    return false;
  }
}