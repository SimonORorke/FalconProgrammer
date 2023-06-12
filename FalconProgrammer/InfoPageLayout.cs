using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using FalconProgrammer.XmlDeserialised;

namespace FalconProgrammer;

public class InfoPageLayout {
  public InfoPageLayout(FalconProgram program) {
    Program = program;
  }

  private int ModWheelReplacementCcNo { get; set; }
  private FalconProgram Program { get; }

  /// <summary>
  ///   If there is a reverb continuous macro and it's MIDI CC number has been
  ///   reassigned to a wheel replacement macro, if there is a delay continuous macro,
  ///   swap the locations of the reverb and delay continuous macros and reassign the
  ///   delay’s MIDI CC number to the reverb.  When and if those changes have been made,
  ///   if there are also a reverb toggle macro and a delay toggle macro, swap their
  ///   locations and MIDI CC numbers.
  /// </summary>
  /// <remarks>
  ///   In many programs, if the reverb continuous macro had the MIDI CC number we want
  ///   to use for a wheel replacement macro, the reverb continuous macro's MIDI CC
  ///   number has been reassigned to the wheel replacement macro, which has been located
  ///   above the reverb continuous macro.  But the reverb continuous macro, though
  ///   seldom used, is more likely to be used than the delay continuous macro, if it
  ///   exists.
  ///   <para>
  ///     So the idea is to to make best use of the delay continuous macro's MIDI CC
  ///     number by reassigning it to the reverb continuous macro.  Their locations need
  ///     to be swapped so that the delay continuous macro, which now has no MIDI CC
  ///     number, is below the wheel replacement macro.  As the locations of the reverb
  ///     and delay continuous macros have been swapped, the locations and MIDI CC
  ///     numbers of the corresponding toggle macros, if any, need to be swapped to, in
  ///     order to keep then next to the continuous macros they enable. 
  ///   </para> 
  /// </remarks>
  public void ReassignDelayMacroCcNoToReverbMacro(int modWheelReplacementCcNo) {
    ModWheelReplacementCcNo = modWheelReplacementCcNo;
    if (FindWheelMacro() == null) {
      return;
    }
    var reverbContinuousMacro = FindReverbContinuousMacroWithNoCcNo();
    if (reverbContinuousMacro == null) {
      return;
    }
    var delayContinuousMacro = FindDelayContinuousMacro();
    if (delayContinuousMacro == null) {
      return;
    }
    SwapMacroLocations(delayContinuousMacro, reverbContinuousMacro);
    string resultMessage = 
      $"'{Program.Name}': Replaced '{delayContinuousMacro.DisplayName}' macro's " + 
      $"MIDI control with Wheel macro MIDI control; swapped '{delayContinuousMacro.DisplayName}' and " + 
      $"'{reverbContinuousMacro.DisplayName}' locations'."; 
    var reverbToggleMacro = FindReverbToggleMacro();
    if (reverbToggleMacro == null) {
      Console.WriteLine(resultMessage);
      return;
    }
    var delayToggleMacro = FindDelayToggleMacro();
    if (delayToggleMacro == null) {
      Console.WriteLine(resultMessage);
      return;
    }
    SwapMacroLocations(delayToggleMacro, reverbToggleMacro);
    resultMessage = 
      resultMessage.TrimEnd('.') + 
      $"; swapped '{delayToggleMacro.DisplayName}' and " +
      $"'{reverbToggleMacro.DisplayName}' locations'.";
    Console.WriteLine(resultMessage);
  }

  /// <summary>
  ///   If feasible, replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro. Otherwise shows a message
  ///   explaining why it is not feasible.
  /// </summary>
  public void ReplaceModWheelWithMacro(
    int modWheelReplacementCcNo, int maxExistingContinuousMacroCount) {
    ModWheelReplacementCcNo = modWheelReplacementCcNo;
    Console.WriteLine($"Checking '{Program.Path}'.");
    if (WheelMacroExists()) {
      return;
    }
    if (!Program.ProgramXml.HasModWheelSignalConnections()) {
      Console.WriteLine($"'{Program.Name}' contains no mod wheel modulations.");
      return;
    }
    var locationForNewWheelMacro =
      FindLocationForNewWheelMacro(maxExistingContinuousMacroCount);
    if (locationForNewWheelMacro == null) {
      return;
    }
    AddWheelMacro(locationForNewWheelMacro.Value);
  }

  private void AddWheelMacro(Point location) {
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
    Program.ProgramXml.AddMacro(wheelMacro);
    Program.ProgramXml.ChangeModWheelSignalConnectionSourcesToMacro(wheelMacro);
    Console.WriteLine(
      $"'{Program.Name}': Replaced mod wheel with macro.");
  }

  private Macro? FindDelayContinuousMacro() {
    return (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.ControlsDelay
      select continuousMacro).FirstOrDefault();
  }

  private Macro? FindDelayToggleMacro() {
    return (
      from macro in Program.Macros
      where macro.ControlsDelay && !macro.IsContinuous
      select macro).FirstOrDefault();
  }

  private void FindDelayOrReverbMacroWithModWheelReplacementCcNo(
    out Macro? delayOrReverbMacroWithWheelCcNo,
    out SignalConnection? delayOrReverbSignalConnectionWithWheelCcNo) {
    delayOrReverbMacroWithWheelCcNo = null;
    delayOrReverbSignalConnectionWithWheelCcNo = null;
    if (Program.InfoPageCcsScriptProcessor != null) {
      var maybeSignalConnection = (
        from signalConnection in Program.InfoPageCcsScriptProcessor.SignalConnections
        where signalConnection.CcNo == ModWheelReplacementCcNo
        select signalConnection).FirstOrDefault();
      if (maybeSignalConnection != null) {
        delayOrReverbMacroWithWheelCcNo = (
          from continuousMacro in Program.ContinuousMacros
          where continuousMacro.MacroNo == maybeSignalConnection.MacroNo
                && (continuousMacro.ControlsDelay || continuousMacro.ControlsReverb)
          select continuousMacro).FirstOrDefault();
        if (delayOrReverbMacroWithWheelCcNo != null) {
          delayOrReverbSignalConnectionWithWheelCcNo = maybeSignalConnection;
        }
      }
    } else {
      foreach (var continuousMacro in Program.ContinuousMacros
                 .Where(continuousMacro => continuousMacro.ControlsDelay
                                           || continuousMacro.ControlsReverb)) {
        delayOrReverbSignalConnectionWithWheelCcNo = (
          from signalConnection in continuousMacro.SignalConnections
          where signalConnection.CcNo == ModWheelReplacementCcNo
          select signalConnection).FirstOrDefault();
        if (delayOrReverbSignalConnectionWithWheelCcNo != null) {
          delayOrReverbMacroWithWheelCcNo = continuousMacro;
          break;
        }
      }
    }
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Point? FindLocationForNewWheelMacro(int maxExistingContinuousMacroCount) {
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
    // Allows a gap above a macro whose display name wraps to two text lines.
    // 95 would be the bare minimum.
    const int verticalClearance = 115;
    int bottomRowY = (
      from macro in Program.Macros
      select macro.Properties.Y).Max();
    var bottomRowMacros = GetBottomRowMacros(bottomRowY);
    bool hasSingleRow = bottomRowMacros.Count == Program.Macros.Count;
    FindDelayOrReverbMacroWithModWheelReplacementCcNo(
      out var delayOrReverbMacroWithWheelCcNo,
      out var delayOrReverbSignalConnectionWithWheelCcNo);
    bool isDelayOrReverbMacroWithWheelCcNoOnBottomRow =
      delayOrReverbMacroWithWheelCcNo != null
      && bottomRowMacros.Contains(delayOrReverbMacroWithWheelCcNo);
    if (Program.ContinuousMacros.Count > maxExistingContinuousMacroCount
        && !isDelayOrReverbMacroWithWheelCcNoOnBottomRow) {
      Console.WriteLine(
        $"'{Program.Name}' " +
        $"has more than {maxExistingContinuousMacroCount} macros "
        + "and no delay/reverb on the bottom row.");
      return null;
    }
    if (isDelayOrReverbMacroWithWheelCcNoOnBottomRow) {
      // Remove the wheel replacement CC number assignment from the delay or reverb
      // macro that has it.  It will be reassigned to the new wheel macro when that is
      // added.
      RemoveSignalConnection(
        delayOrReverbMacroWithWheelCcNo!, delayOrReverbSignalConnectionWithWheelCcNo!);
      // Locate the new wheel macro above the delay or reverb macro.
      return new Point(
        delayOrReverbMacroWithWheelCcNo!.Properties.X,
        delayOrReverbMacroWithWheelCcNo.Properties.Y - verticalClearance);
    }
    // There is no wheel replacement CC number on a delay or reverb macro to reassign.
    //
    // List, from left to right, the widths of the gaps between the macros on the bottom
    // row of macros on the Info page.  Include the gap between the leftmost macro and
    // the left edge and the gap between the rightmost macro and the right edge.
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
    // Check whether there any gaps on the bottom rowe wide enough to accommodate a new
    // macro.
    bool canFitInGap = (
      from gapWidth in gapWidths
      where gapWidth >= minNewMacroGapWidth
      select gapWidth).Any();
    if (!canFitInGap) {
      if (!hasSingleRow) {
        // No instances of this have been reported.
        Console.WriteLine(
          $"'{Program.Name}' " +
          "does not have room on its Info page's bottom row for a new macro.");
        return null;
      }
      // Locate the new wheel macro above the rightmost macro.
      // Example: Factory/Bass-Sub/Gamma Bass 1.4.
      var rightmostMacro = bottomRowMacros[^1];
      return new Point(
        rightmostMacro.Properties.X,
        rightmostMacro.Properties.Y - verticalClearance);
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
      : bottomRowMacros[newMacroGapIndex - 1].Properties.X + macroWidth;
    int newMacroX = newMacroGapX + (rightmostSuitableGapWidth - macroWidth) / 2;
    // If there are continuous and toggle macros on the bottom row, the continuous macros
    // may be a little higher up than the toggle macros, as they are taller.  In that
    // case, align the new macro horizontally with the bottommost continuous macro.
    // Example: "Factory\Pluck\Mutan Mute".
    var bottomRowContinuousMacros = (
      from macro in bottomRowMacros
      where macro.IsContinuous
      select macro).ToList();
    int newMacroY;
    if (bottomRowContinuousMacros.Count > 0) {
      newMacroY = (
        from macro in bottomRowContinuousMacros
        select macro.Properties.Y).Max();
    } else {
      newMacroY = bottomRowY <= standardBottommostY ? bottomRowY : standardBottommostY;
    }
    return new Point(newMacroX, newMacroY);
  }

  private Macro? FindReverbContinuousMacroWithNoCcNo() {
    return (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.ControlsReverb
            && continuousMacro.SignalConnections.Count == 0
      select continuousMacro).FirstOrDefault();
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

  private void RemoveSignalConnection(Macro macro, SignalConnection signalConnection) {
    if (Program.InfoPageCcsScriptProcessor != null) {
      if (Program.InfoPageCcsScriptProcessor.SignalConnections
          .Contains(signalConnection)) {
        Program.InfoPageCcsScriptProcessor.SignalConnections.Remove(signalConnection);
      }
    }
    if (macro.SignalConnections.Contains(signalConnection)) {
      macro.SignalConnections.Remove(signalConnection);
    }
    Program.ProgramXml.RemoveSignalConnectionElementsWithSource(
      signalConnection.Source);
  }

  private void SwapMacroCcNos(Macro macro1, Macro macro2) {
    SignalConnection? signalConnection1;
    SignalConnection? signalConnection2;
    if (Program.InfoPageCcsScriptProcessor != null) {
      signalConnection1 = (
        from signalConnection in Program.InfoPageCcsScriptProcessor.SignalConnections
        where signalConnection.MacroNo == macro1.MacroNo
        select signalConnection).FirstOrDefault();
      signalConnection2 = (
        from signalConnection in Program.InfoPageCcsScriptProcessor.SignalConnections
        where signalConnection.MacroNo == macro2.MacroNo
        select signalConnection).FirstOrDefault();
      if (signalConnection1 != null || signalConnection2 != null) {
        if (signalConnection1 != null && signalConnection2 != null) {
          signalConnection1.MacroNo = macro2.MacroNo;
          signalConnection2.MacroNo = macro1.MacroNo;
        } else if (signalConnection1 != null) {
          // signalConnection2 is null
          signalConnection1.MacroNo = macro2.MacroNo;
        } else {
          // signalConnection1 is null
          // signalConnection2 is not null
          signalConnection2!.MacroNo = macro1.MacroNo;
        }
        Program.ProgramXml.UpdateInfoPageCcsScriptProcessor();
        return;
      }
    }
    // ReSharper disable once ConvertIfStatementToSwitchStatement
    if (macro1.SignalConnections.Count == 0 && macro2.SignalConnections.Count == 0) {
      // This happens if the signal connections belongs to effects, a scenario we don't
      // (yet) support. Example: 'Factory/Pads/Lush Chords 2.0'.
      Console.WriteLine(
        $"'{Program.Name}': Cannot find SignalConnections of supported types for " + 
        $"either macro '{macro1.DisplayName}' or macro '{macro2.DisplayName}'.");
    }
    // SignalConnections belong to Macros.
    signalConnection1 = null;
    signalConnection2 = null;
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