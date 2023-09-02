using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class InfoPageLayout {
  private const int MacroWidth = 60;
  // private const int MinHorizontalGapBetweenMacros = 5;
  private const int RightEdge = 695;

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

  /// <summary>
  ///   Allows a gap above a macro whose display name wraps to two text lines.
  ///   95 would be the bare minimum.
  /// </summary>
  private const int StandardRowHeight = 115;

  public InfoPageLayout(FalconProgram program) {
    Program = program;
  }

  /// <summary>
  ///   Gets or sets the maximum number of continuous macros, in MIDI CC number order,
  ///   there can be on the Info page before the MIDI CC number that is specified by
  ///   <see cref="ModWheelReplacementCcNo" /> and used for the modulation wheel
  ///   replacement macro that can be added to the layout by
  ///   <see cref="ReplaceModWheelWithMacro" />.
  /// </summary>
  [PublicAPI]
  public int MaxContinuousMacroCountBeforeWheelMacro { get; set; } = 3;

  /// <summary>
  ///   Gets or sets the MIDI CC number to be mapped to a program's new mod wheel
  ///   replacement macro by <see cref="ReplaceModWheelWithMacro" />.
  /// </summary>
  [PublicAPI]
  public int ModWheelReplacementCcNo { get; set; } = 34;

  private FalconProgram Program { get; }

  public void MoveMacrosToStandardLayout() {
    var visibleMacros = (
      from macro in Program.Macros
      // Exclude invisible macros.
      // See comment in Program.GetMacrosSortedByLocation, which uses a
      // different approach to identify them.
      where macro.X < RightEdge
      select macro).ToList();
    const int maxMacroCount = 20;
    if (visibleMacros.Count > maxMacroCount) {
      throw new InvalidOperationException(
        $"{Program.PathShort}: Cannot lay out {visibleMacros.Count} macros. " +
        $"The maximum is {maxMacroCount}.");
    }
    int macrosPerRow = visibleMacros.Count <= 16 ? 4 : 5;
    int rowCount = (int)Math.Ceiling((double)visibleMacros.Count / macrosPerRow);
    int rowHeight = rowCount < 4 ? StandardRowHeight : StandardRowHeight - 5; 
    int freeSpaceInRow = RightEdge - MacroWidth * macrosPerRow;
    int gapBetweenMacros = freeSpaceInRow / (macrosPerRow + 1);
    int top = rowCount switch {
      1 => StandardBottommostY - rowHeight,
      2 => StandardBottommostY - 2 * rowHeight,
      3 => StandardBottommostY - 2 * rowHeight,
      _ => StandardBottommostY - 3 * rowHeight
    };
    switch (Program.Category.SoundBankFolder.Name) {
      case "Ether Fields" when rowCount == 3:
        top -= 85;
        break;
      case "Devinity" when rowCount == 1:
        top += rowHeight;
        break;
      default:
        top -= 5;
        break;
    }
    int x = gapBetweenMacros;
    int y = top;
    int macrosOnCurrentRow = 0;
    foreach (var macro in visibleMacros) {
      macrosOnCurrentRow++;
      macro.X = x;
      macro.Y = y;
      if (macrosOnCurrentRow < macrosPerRow) {
        x += gapBetweenMacros + MacroWidth;
      } else {
        macrosOnCurrentRow = 0;
        x = gapBetweenMacros;
        y += StandardRowHeight;
      }
    }
    Console.WriteLine($"{Program.PathShort}: Moved macros to standard layout.");
  }

  /// <summary>
  ///   Replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro.
  /// </summary>
  public void ReplaceModWheelWithMacro() {
    OrderMacrosByLocation();
    var wheelMacro = CreateWheelMacro();
    var visibleContinuousMacros = (
      from macro in Program.ContinuousMacros
      where macro.IsContinuous
            // Exclude invisible macros
            && macro.X < RightEdge
      select macro).ToList();
    int insertionIndex = visibleContinuousMacros.Count switch {
      0 => 0, // First
      < 4 => Program.Macros.IndexOf(visibleContinuousMacros[^1]) + 1, // Last continuous
      _ => Program.Macros.IndexOf(visibleContinuousMacros[3]) // 4th continuous
    };
    Program.Macros.Insert(insertionIndex, wheelMacro);
    Program.RefreshMacroOrder();
    MoveMacrosToStandardLayout();
  }

  private Macro CreateWheelMacro() {
    int wheelMacroNo = Program.Macros.Count > 1
      ? (
        from macro in Program.Macros
        select macro.MacroNo).Max() + 1
      // ReSharper disable once CommentTypo
      // Example: Factory\Distorted\Doom Octaver after it has had its Delay macro removed.
      : 1;
    var result = new Macro(Program.ProgramXml) {
      MacroNo = wheelMacroNo,
      DisplayName = "Wheel",
      Bipolar = 0,
      CustomPosition = true,
      IsContinuous = true,
      Value = 0
    };
    result.AddModulation(new Modulation(Program.ProgramXml) {
      CcNo = ModWheelReplacementCcNo
    });
    result.ChangeModWheelModulationSourcesToMacro();
    return result;
  }

  private void OrderMacrosByLocation() {
    var visibleMacrosSortedByLocation = Program.GetMacrosSortedByLocation(
      Program.MacroCcLocationOrder);
    var newOrder = new List<Macro>();
    newOrder.AddRange(visibleMacrosSortedByLocation);
    var invisibleMacros =
      from macro in Program.Macros
      where !newOrder.Contains(macro)
      select macro;
    newOrder.AddRange(invisibleMacros);
    Program.Macros.Clear();
    Program.Macros.AddRange(newOrder);
    Program.RefreshMacroOrder();
  }
}