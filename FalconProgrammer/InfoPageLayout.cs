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
    const int verticalClearance = 115; 
    int bottomRowY = (
      from macro in Program.Macros
      select macro.Properties.Y).Max();
    var bottomRowMacros = GetBottomRowMacros(bottomRowY);
    bool layoutHasSingleRow = bottomRowMacros.Count == Program.Macros.Count;
    FindDelayOrReverbMacroWithModWheelReplacementCcNo(
      out var delayOrReverbMacroWithWheelCcNo,
      out var delayOrReverbSignalConnectionWithWheelCcNo);
    bool delayOrReverbMacroWithWheelCcNoIsOnBottomRow =
      delayOrReverbMacroWithWheelCcNo != null
      && bottomRowMacros.Contains(delayOrReverbMacroWithWheelCcNo);
    if (Program.ContinuousMacros.Count > maxExistingContinuousMacroCount
        && !(layoutHasSingleRow && delayOrReverbMacroWithWheelCcNoIsOnBottomRow)) {
      Console.WriteLine(
        $"'{Program.Name}' " +
        "does not have room on its Info page for a Wheel macro.");
      return null;
    }
    if (layoutHasSingleRow && delayOrReverbMacroWithWheelCcNoIsOnBottomRow) {
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
    // There is no wheel replacement CC number to reassign.
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
      if (layoutHasSingleRow) {
        // Locate the new wheel macro above the rightmost macro.
        var rightmostMacro = bottomRowMacros[^1];
        return new Point(
          rightmostMacro.Properties.X,
          rightmostMacro.Properties.Y - verticalClearance);
      } else {
        Console.WriteLine(
          $"'{Program.Name}' " +
          "does not have room on its Info page's bottom row for a new macro.");
      }
      return null;
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

  private bool WheelMacroExists() {
    string? existingWheelMacroDisplayName = (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.DisplayName.ToLower().Contains("wheel")
      select continuousMacro.DisplayName).FirstOrDefault();
    if (existingWheelMacroDisplayName != null) {
      Console.WriteLine(
        $"'{Program.Name}' already has a '{existingWheelMacroDisplayName}' macro.");
      return true;
    }
    return false;
  }
}