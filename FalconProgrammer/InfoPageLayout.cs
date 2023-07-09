using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class InfoPageLayout {
  private const int MacroWidth = 60;
  private const int MinHorizontalGapBetweenMacros = 5;

  /// <summary>
  ///   When there are only toggle macros on the bottom row, they may be lower than the
  ///   standard bottom, usually to accomodate two-line display names.  This looks OK for
  ///   toggle macros.  But for continuous macros, being taller, it makes an ugly lack of
  ///   bottom margin.  So place the new continuous macro no lower than the standard
  ///   bottommost Y.
  ///   ReSharper disable once CommentTypo
  ///   Example: "Factory\Pluck\Pad Mullerizer".
  /// </summary>
  private const int StandardBottommostY = 355;

  private const int RightEdge = 695;

  public InfoPageLayout(FalconProgram program) {
    Program = program;
  }

  private List<Macro> BottomRowMacros { get; set; } = null!;
  private int BottomRowY { get; set; }

  /// <summary>
  ///   Gets or sets the maximum number of continuous macros, in MIDI CC number order,
  ///   there can be on the Info page before the MIDI CC number that is specified by
  ///   <see cref="ModWheelReplacementCcNo" /> and used for the modulation wheel
  ///   replacement macro tha can be added to the layout by
  ///   <see cref="TryReplaceModWheelWithMacro" />.
  /// </summary>
  [PublicAPI]
  public int MaxContinuousMacroCountBeforeWheelMacro { get; set; } = 3;

  /// <summary>
  ///   Gets or sets the MIDI CC number to be mapped to a program's new mod wheel
  ///   replacement macro by <see cref="TryReplaceModWheelWithMacro" />.
  /// </summary>
  [PublicAPI]
  public int ModWheelReplacementCcNo { get; set; } = 34;

  private FalconProgram Program { get; }

  public void MoveAllMacrosToStandardBottom() {
    int freeSpace = RightEdge - MacroWidth * Program.Macros.Count;
    int gapBetweenMacros = freeSpace / (Program.Macros.Count + 1);
    if (gapBetweenMacros < MinHorizontalGapBetweenMacros) {
      throw new ApplicationException(
        $"{Program.PathShort}: There is not enough horizontal space to move " + 
        $"{Program.Macros.Count} macros to the standard bottom row.");
    }
    // Move any reverb and delay macros to the right end of the standard bottom row
    // This will allow the standard wheel replacement MIDI CC number to be reassigned to
    // the wheel replacement macro from the continuous  macro I'm least likely to use,
    // preferably delay, otherwise reverb.
    // Example: Savage\Leads\Saw Dirty.
    MoveMacroToEndIfExists(FindReverbToggleMacro());
    MoveMacroToEndIfExists(FindReverbContinuousMacro());
    MoveMacroToEndIfExists(FindDelayToggleMacro());
    MoveMacroToEndIfExists(FindDelayContinuousMacro());
    Program.ProgramXml.ReplaceMacroElements(Program.Macros);
    int x = gapBetweenMacros;
    foreach (var macro in Program.Macros) {
      macro.Properties.X = x;
      macro.Properties.Y = StandardBottommostY;
      Program.ProgramXml.UpdateMacroLocation(macro);
      x += gapBetweenMacros + MacroWidth;
    }
  }

  /// <summary>
  ///   If feasible, replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro. Otherwise shows a message
  ///   explaining why it is not feasible.
  /// </summary>
  public bool TryReplaceModWheelWithMacro(out bool updateMacroCcs) {
    var locationForNewWheelMacro = FindLocationForNewWheelMacro(
      out updateMacroCcs);
    // Debug.WriteLine("================================================");
    // Debug.WriteLine("After FindLocationForNewWheelMacro");
    // foreach (var macro in Program.Macros) {
    //   foreach (var signalConnection in macro.SignalConnections) {
    //     Debug.WriteLine($"{macro}, CcNo {signalConnection.CcNo}");
    //   }
    // }
    // Debug.WriteLine("================================================");
    if (locationForNewWheelMacro == null) {
      return false;
    }
    AddWheelMacro(locationForNewWheelMacro.Value);
    return true;
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
      ProgramXml = Program.ProgramXml,
      SignalConnections = new List<SignalConnection> {
        new SignalConnection {
          CcNo = ModWheelReplacementCcNo
        }
      },
      Properties = new MacroProperties {
        X = location.X,
        Y = location.Y
      }
    };
    Program.Macros.Add(wheelMacro);
    Program.ProgramXml.AddMacroElement(wheelMacro);
    Program.ProgramXml.ChangeModWheelSignalConnectionSourcesToMacro(wheelMacro);
  }

  private Macro? FindContinuousMacroWithModWheelReplacementCcNo() {
    return (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.FindSignalConnectionWithCcNo(ModWheelReplacementCcNo) != null 
      select continuousMacro).FirstOrDefault();
  }

  public Macro? FindDelayContinuousMacro() {
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
      // There could also be a mod wheel CC 1, so we cannot assume it's the first
      // SignalConnection. Example: Titanium\Pads\Children's Choir.
      delayOrReverbSignalConnectionWithWheelCcNo = continuousMacro.FindSignalConnectionWithCcNo(
        ModWheelReplacementCcNo);
    }
  }

  private Macro? FindDelayToggleMacro() {
    return (
      from macro in Program.Macros
      where macro.ControlsDelay && !macro.IsContinuous
      select macro).FirstOrDefault();
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Point? FindLocationForNewWheelMacro(out bool updateMacroCcs) {
    updateMacroCcs = false;
    BottomRowY = (
      from macro in Program.Macros
      select macro.Properties.Y).Max();
    BottomRowMacros = GetBottomRowMacros(BottomRowY);
    var result = LocateWheelAboveDelayOrReverbMacro();
    if (result != null) {
      // Debug.WriteLine("================================================");
      // Debug.WriteLine("After LocateWheelAboveDelayOrReverbMacro");
      // foreach (var macro in Program.Macros) {
      //   foreach (var signalConnection in macro.SignalConnections) {
      //     Debug.WriteLine($"{macro}, CcNo {signalConnection.CcNo}");
      //   }
      // }
      // Debug.WriteLine("================================================");
      // If the delay or reverb macro that has lost its CC is the last continuous macro,
      // it can have a new one. Example Factory\Bass-Sub\BA-Shomp 1.2.
      // Counter-example, where the delay or reverb macro must not get a new CC:
      // Factory\Keys\Dirty Toy Piano 1.1.
      if (Program.ContinuousMacros.Count == MaxContinuousMacroCountBeforeWheelMacro + 1) {
        updateMacroCcs = true;
      }
      return result;
    }
    // There is no wheel replacement CC number on a delay or reverb macro to reassign.
    result = LocateWheelMacroOnOrAboveBottomRow();
    if (result != null) {
      return result;
    }
    // There is no wheel replacement CC number on a delay or reverb macro to reassign
    // and there are more than {MaxContinuousMacroCountBeforeWheelMacro} continuous macros.
    result = LocateWheelMacroAboveMacroWithWheelCcNo();
    if (result != null) {
      updateMacroCcs = true;
    }
    return result;
  }

  public Macro? FindReverbContinuousMacro() {
    return (
      from macro in Program.ContinuousMacros
      where macro.ControlsReverb
      select macro).FirstOrDefault();
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

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Point LocateNewMacroAboveMacro(Macro macro) {
    // Allows a gap above a macro whose display name wraps to two text lines.
    // 95 would be the bare minimum.
    const int verticalClearance = 115;
    var result = new Point(
      macro.Properties.X,
      macro.Properties.Y - verticalClearance);
    var overlappingMacro = (
      from otherMacro in Program.Macros
      where otherMacro.Properties.Y > result.Y - 50
            && otherMacro.Properties.Y < result.Y + 50
            && otherMacro.Properties.X > result.X - MacroWidth
            && otherMacro.Properties.X < result.X + MacroWidth
      select otherMacro).FirstOrDefault();
    if (overlappingMacro != null) {
      // Example: Spectre\Polysynth\PL Cream.
      result = new Point(
        macro.Properties.X,
        overlappingMacro.Properties.Y - verticalClearance);
    }
    return result;
  }

  private Point? LocateWheelAboveDelayOrReverbMacro() {
    SwapDelayAndReverbIfReverbHasWheelReplacementCcNo();
    // Debug.WriteLine("================================================");
    // Debug.WriteLine("After SwapDelayAndReverbIfReverbHasWheelReplacementCcNo");
    // foreach (var macro in Program.Macros) {
    //   foreach (var signalConnection in macro.SignalConnections) {
    //     Debug.WriteLine($"{macro}, CcNo {signalConnection.CcNo}");
    //   }
    // }
    // Debug.WriteLine("================================================");
    FindDelayOrReverbMacroWithModWheelReplacementCcNo(
      out var delayOrReverbMacroWithWheelCcNo,
      out var delayOrReverbSignalConnectionWithWheelCcNo);
    // bool isDelayOrReverbMacroWithWheelCcNoOnBottomRow =
    //   delayOrReverbMacroWithWheelCcNo != null
    //   && BottomRowMacros.Contains(delayOrReverbMacroWithWheelCcNo);
    // if (isDelayOrReverbMacroWithWheelCcNoOnBottomRow) {
    if (delayOrReverbMacroWithWheelCcNo != null) {
      // Debug.WriteLine("================================================");
      // Debug.WriteLine("After FindDelayOrReverbMacroWithModWheelReplacementCcNo");
      // foreach (var macro in Program.Macros) {
      //   foreach (var signalConnection in macro.SignalConnections) {
      //     Debug.WriteLine($"{macro}, CcNo {signalConnection.CcNo}");
      //   }
      // }
      // Debug.WriteLine("================================================");
      // Remove the wheel replacement CC number assignment from the delay or reverb
      // macro that has it.  It will be reassigned to the new wheel macro when that is
      // added.
      delayOrReverbMacroWithWheelCcNo.RemoveSignalConnection(
        delayOrReverbSignalConnectionWithWheelCcNo!);
      // Locate the new wheel macro above the delay or reverb macro.
      // Debug.WriteLine("================================================");
      // Debug.WriteLine("After RemoveSignalConnection");
      // foreach (var macro in Program.Macros) {
      //   foreach (var signalConnection in macro.SignalConnections) {
      //     Debug.WriteLine($"{macro}, CcNo {signalConnection.CcNo}");
      //   }
      // }
      // Debug.WriteLine("================================================");
      return LocateNewMacroAboveMacro(delayOrReverbMacroWithWheelCcNo);
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
    if (Program.ContinuousMacros.Count > MaxContinuousMacroCountBeforeWheelMacro) {
      return null;
    }
    const int minNewMacroGapWidth = MacroWidth + 2 * MinHorizontalGapBetweenMacros;
    // List, from left to right, the widths of the gaps between the macros on the bottom
    // row of macros on the Info page.  Include the gap between the leftmost macro and
    // the left edge and the gap between the rightmost macro and the right edge.
    var gapWidths = new List<int> { BottomRowMacros[0].Properties.X };
    if (BottomRowMacros.Count > 1) {
      for (int i = 0; i < BottomRowMacros.Count - 1; i++) {
        gapWidths.Add(
          BottomRowMacros[i + 1].Properties.X
          - (BottomRowMacros[i].Properties.X
             + MacroWidth));
      }
    }
    gapWidths.Add(RightEdge - (BottomRowMacros[^1].Properties.X + MacroWidth));
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
      : BottomRowMacros[newMacroGapIndex - 1].Properties.X + MacroWidth;
    int newMacroX = newMacroGapX + (rightmostSuitableGapWidth - MacroWidth) / 2;
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
      newMacroY = BottomRowY <= StandardBottommostY ? BottomRowY : StandardBottommostY;
    }
    return new Point(newMacroX, newMacroY);
  }

  private void MoveMacroToEndIfExists(Macro? macroIfExists) {
    if (macroIfExists != null && macroIfExists != Program.Macros[^1]) {
      Program.Macros.Remove(macroIfExists);
      Program.Macros.Add(macroIfExists);
    }
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
    // if (reverbContinuousMacro != null
    //     && BottomRowMacros.Contains(reverbContinuousMacro)) {
    if (reverbContinuousMacro != null) {
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
      macro1.RemoveSignalConnection(signalConnection1);
    }
    if (macro2.SignalConnections.Count > 0) {
      signalConnection2 = macro2.SignalConnections[0];
      macro2.RemoveSignalConnection(signalConnection2);
    }
    if (signalConnection1 != null) {
      macro2.AddSignalConnection(signalConnection1);
    }
    if (signalConnection2 != null) {
      macro1.AddSignalConnection(signalConnection2);
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
}