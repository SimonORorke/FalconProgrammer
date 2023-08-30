using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using FalconProgrammer.XmlLinq;
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
      throw new InvalidOperationException(
        $"{Program.PathShort}: There is not enough horizontal space to move " + 
        $"{Program.Macros.Count} macros to the standard bottom row.");
    }
    // Move any reverb to the right end of the standard bottom row
    // This will allow the standard wheel replacement MIDI CC number to be reassigned to
    // the wheel replacement macro from the continuous  macro I'm least likely to use,
    // preferably delay, otherwise reverb.
    // Example: Savage\Leads\Saw Dirty.
    int x = gapBetweenMacros;
    foreach (var macro in Program.Macros) {
      macro.X = x;
      macro.Y = StandardBottommostY;
      x += gapBetweenMacros + MacroWidth;
    }
    Console.WriteLine($"{Program.PathShort}: Moved all macros to bottom.");
  }

  /// <summary>
  ///   If feasible, replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro. Otherwise shows a message
  ///   explaining why it is not feasible.
  /// </summary>
  public bool TryReplaceModWheelWithMacro(out bool updateMacroCcs) {
    var locationForNewWheelMacro = FindLocationForNewWheelMacro(
      out updateMacroCcs);
    if (locationForNewWheelMacro == null) {
      return false;
    }
    AddWheelMacro(locationForNewWheelMacro.Value);
    return true;
  }

  private void AddWheelMacro(Point location) {
    int wheelMacroNo = Program.Macros.Count > 1 
      ? (
      from macro in Program.Macros
      select macro.MacroNo).Max() + 1
      // ReSharper disable once CommentTypo
      // Example: Factory\Distorted\Doom Octaver after it has had its Delay macro removed.
      : 1;
    var wheelMacro = new Macro(Program.ProgramXml) {
      MacroNo = wheelMacroNo,
      DisplayName = "Wheel",
      Bipolar = 0,
      CustomPosition = true,
      IsContinuous = true,
      Value = 0,
      X = location.X,
      Y = location.Y
    };
    wheelMacro.AddModulation(new Modulation(Program.ProgramXml) {
      CcNo = ModWheelReplacementCcNo
    });
    wheelMacro.ChangeModWheelModulationSourcesToMacro();
    Program.Macros.Add(wheelMacro);
  }

  private Macro? FindContinuousMacroWithModWheelReplacementCcNo() {
    return (
      from continuousMacro in Program.ContinuousMacros
      where continuousMacro.FindModulationWithCcNo(ModWheelReplacementCcNo) != null 
      select continuousMacro).FirstOrDefault();
  }

  private void FindDelayOrReverbMacroWithModWheelReplacementCcNo(
    out Macro? delayOrReverbMacroWithWheelCcNo,
    out Modulation? delayOrReverbModulationWithWheelCcNo) {
    delayOrReverbMacroWithWheelCcNo = null;
    delayOrReverbModulationWithWheelCcNo = null;
    var continuousMacro = FindContinuousMacroWithModWheelReplacementCcNo();
    if (continuousMacro != null &&
        (continuousMacro.ModulatesDelay || continuousMacro.ModulatesReverb)) {
      delayOrReverbMacroWithWheelCcNo = continuousMacro;
      // There could also be a mod wheel CC 1, so we cannot assume it's the first
      // Modulation. Example: Titanium\Pads\Children's Choir.
      delayOrReverbModulationWithWheelCcNo = continuousMacro.FindModulationWithCcNo(
        ModWheelReplacementCcNo);
    }
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Point? FindLocationForNewWheelMacro(out bool updateMacroCcs) {
    updateMacroCcs = false;
    if (Program.Macros.Count == 0) {
      // Example: Factory\Distorted\Doom Octaver after it has had its Delay macro removed.
      updateMacroCcs = true;
      return new Point(0, StandardBottommostY);
    }
    var sortedByLocation = Program.GetMacrosSortedByLocation(
      LocationOrder.TopToBottomLeftToRight);
    BottomRowY = (
      from macro in sortedByLocation
      select macro.Y).Max();
    BottomRowMacros = GetBottomRowMacros(sortedByLocation);
    var result = LocateWheelAboveDelayOrReverbMacro();
    if (result != null) {
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

  /// <summary>
  ///   Returns, from left to right, the macros on the bottom row of macros on the Info
  ///   page.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  private List<Macro> GetBottomRowMacros(IEnumerable<Macro> macrosSortedByLocation) {
    // We need to horizontally align the new macro relative not only to macros that are
    // bottommost on the Info window (i.e. highest Y) but also those that are close to
    // the bottom.  The vertical clearance is 95, so this should be safe. In reality,
    // many are up just 5 from the bottommost macros.
    // Example: "Factory\Pluck\Mutan Mute".
    const int verticalFudge = 50;
    return (
      from macro in macrosSortedByLocation
      where macro.Y >= BottomRowY - verticalFudge
      select macro).ToList();
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Point LocateNewMacroAboveMacro(Macro macro) {
    // Allows a gap above a macro whose display name wraps to two text lines.
    // 95 would be the bare minimum.
    const int verticalClearance = 115;
    var result = new Point(
      macro.X,
      macro.Y - verticalClearance);
    var overlappingMacro = (
      from otherMacro in Program.Macros
      where otherMacro.Y > result.Y - 50
            && otherMacro.Y < result.Y + 50
            && otherMacro.X > result.X - MacroWidth
            && otherMacro.X < result.X + MacroWidth
      select otherMacro).FirstOrDefault();
    if (overlappingMacro != null) {
      // Example: Spectre\Polysynth\PL Cream.
      result = new Point(
        macro.X,
        overlappingMacro.Y - verticalClearance);
    }
    return result;
  }

  private Point? LocateWheelAboveDelayOrReverbMacro() {
    FindDelayOrReverbMacroWithModWheelReplacementCcNo(
      out var delayOrReverbMacroWithWheelCcNo,
      out var delayOrReverbModulationWithWheelCcNo);
    if (delayOrReverbMacroWithWheelCcNo != null) {
      // Remove the wheel replacement CC number assignment from the delay or reverb
      // macro that has it.  It will be reassigned to the new wheel macro when that is
      // added.
      delayOrReverbMacroWithWheelCcNo.RemoveModulation(
        delayOrReverbModulationWithWheelCcNo!);
      // Locate the new wheel macro above the delay or reverb macro.
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
    var gapWidths = new List<int> { BottomRowMacros[0].X };
    if (BottomRowMacros.Count > 1) {
      for (int i = 0; i < BottomRowMacros.Count - 1; i++) {
        gapWidths.Add(
          BottomRowMacros[i + 1].X
          - (BottomRowMacros[i].X
             + MacroWidth));
      }
    }
    gapWidths.Add(RightEdge - (BottomRowMacros[^1].X + MacroWidth));
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
      : BottomRowMacros[newMacroGapIndex - 1].X + MacroWidth;
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
        select macro.Y).Max();
    } else {
      newMacroY = BottomRowY <= StandardBottommostY ? BottomRowY : StandardBottommostY;
    }
    return new Point(newMacroX, newMacroY);
  }
}