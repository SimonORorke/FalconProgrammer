﻿using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Model;

internal class InfoPageLayout {
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
  ///   Example: "Falcon Factory\Pluck\Pad Mullerizer".
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

  private List<Macro> Macros => Program.Macros;
  private FalconProgram Program { get; }
  private ProgramXml ProgramXml => Program.ProgramXml;

  /// <summary>
  ///   Moves the specified macros to the end of the layout of macros on the Info page.
  /// </summary>
  /// <remarks>
  ///   After one or more calls to <see cref="MoveMacroToEnd" />,
  ///   <see cref="RefreshMacroOrder" /> must be called.
  /// </remarks>
  public void MoveMacroToEnd(Macro macro) {
    if (macro != Macros[^1]) {
      Macros.Remove(macro);
      Macros.Add(macro);
      Program.NotifyUpdate(
        $"{Program.PathShort}: Moved {macro.DisplayNameWithoutCc} macro to end.");
    }
  }

  public void MoveMacrosToStandardLayout() {
    var visibleMacros = (
      from macro in Macros
      // Exclude invisible macros.
      // See comment in Program.GetMacrosSortedByLocation, which uses a
      // different approach to identify them.
      where macro.X < RightEdge
      select macro).ToList();
    const int maxMacroCount = 32; 
    if (visibleMacros.Count > maxMacroCount) {
      throw new ApplicationException(
        $"{Program.PathShort}: Cannot lay out {visibleMacros.Count} macros. " +
        $"The maximum is {maxMacroCount}.");
    }
    int macrosPerRow = visibleMacros.Count switch {
      <= 12 => 4,
      <= 15 => 5,
      <= 21 => 7,
      _ => 8
    };
    int rowCount = (int)Math.Ceiling((double)visibleMacros.Count / macrosPerRow);
    int rowHeight = rowCount < 4 ? StandardRowHeight : StandardRowHeight - 10;
    int freeSpaceInRow = RightEdge - MacroWidth * macrosPerRow;
    int gapBetweenMacros = freeSpaceInRow / (macrosPerRow + 1);
    int top = rowCount switch {
      1 => StandardBottommostY - rowHeight,
      2 => StandardBottommostY - 2 * rowHeight,
      3 => StandardBottommostY - 2 * rowHeight,
      _ => StandardBottommostY - 3 * rowHeight
    };
    switch (Program.SoundBankId) {
      case SoundBankId.EtherFields when rowCount == 3:
        if (ProgramXml.BackgroundImagePath != null 
            && ProgramXml.BackgroundImagePath.StartsWith("$Ether Fields.ufs")) {
          // Default background image. Avoid the text at bottom.
          top -= 85;
        }
        break;
      case SoundBankId.Devinity when rowCount == 1:
        if (ProgramXml.BackgroundImagePath != null 
            && ProgramXml.BackgroundImagePath.StartsWith("$Devinity.ufs")) {
          // Default background image. Place the row on the black space at the bottom. 
          top += rowHeight;
        }
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
        y += rowHeight;
      }
    }
    Program.NotifyUpdate(
      $"{Program.PathShort}: Moved macros to standard layout.");
  }

  /// <summary>
  ///   Replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro.
  /// </summary>
  /// <remarks>
  ///   We want the 4 (main) expression pedals to control the four most important macros.
  ///   So in general the wheel macro will become the fourth continuous macro or, if
  ///   there are fewer than four existing continuous macros, the last.
  ///   <para>
  ///     However, when there are 4 macros and the are all continuous, they tend to be
  ///     more important than the wheel macro, unless a reverb macro has been zeroed and
  ///     moved to the end by <see cref="FalconProgram.MoveZeroedMacrosToEnd" />.
  ///     Several sound banks follow this pattern. So, in that scenario, insert the wheel
  ///     macro before any zeroed reverb macro if that is last, or otherwise add the
  ///     wheel macro to the end. Once <see cref="FalconProgram.ReuseCc1" /> has been
  ///     run, the mod wheel will control the 5th macro via MIDI CC 1.
  ///     If any of the four existing macros are toggles, there's nothing to do,
  ///     as toggle macros are not controlled by expression pedals.
  ///   </para>
  /// </remarks>
  public void ReplaceModWheelWithMacro() {
    OrderMacrosByLocation();
    var wheelMacro = CreateWheelMacro();
    var visibleContinuousMacros = (
      from macro in Program.ContinuousMacros
      where macro.IsContinuous
            // Exclude invisible macros
            && macro.X < RightEdge
      select macro).ToList();
    int insertionIndex;
    var adsrMacros = Program.GetAdsrMacros();
    if (adsrMacros.Count != 4) {
      insertionIndex = visibleContinuousMacros.Count switch {
        0 => 0, // First and only
        < 4 => AtEnd(),
        4 => IsLastMacroZeroedReverb() && NoToggleMacros()
          // Examples:
          // Devinity\Plucks-Leads\Pluck Sphere (reverb at end in original)
          // Eternal Funk\Brass\Back And Stride (reverb moved to end by ZeroAndMoveMacros)
          ? Fourth()
          : AtEnd(), // Example: Eternal Funk\Synths\Bell Shaka 
        _ => Fourth()
      };
    } else { // Insert Wheel macro before ADSR macros
      // Examples: many Eternal Funk programs; Ether Fields\Hybrid\Cine Guitar Pad.
      insertionIndex = visibleContinuousMacros.IndexOf(adsrMacros["Attack"]);
    }
    Macros.Insert(insertionIndex, wheelMacro);
    RefreshMacroOrder();
    MoveMacrosToStandardLayout();
    Program.UpdateMacroCcs();
    Program.NotifyUpdate($"{Program.PathShort}: Replaced mod wheel with macro.");
    return;

    int AtEnd() {
      return Macros.IndexOf(visibleContinuousMacros[^1]) + 1;
    }

    int Fourth() {
      return Macros.IndexOf(visibleContinuousMacros[3]);
    }

    bool IsLastMacroZeroedReverb() {
      return visibleContinuousMacros[^1].ModulatesReverb &&
             visibleContinuousMacros[^1].Value == 0;
    }

    bool NoToggleMacros() {
      return Macros.Count == visibleContinuousMacros.Count;
    }
  }

  private Macro CreateWheelMacro() {
    int wheelMacroNo = Macros.Count > 1
      ? (
        from macro in Macros
        select macro.MacroNo).Max() + 1
      // ReSharper disable once CommentTypo
      // Example: Falcon Factory\Distorted\Doom Octaver after it has had its Delay macro removed.
      : 1;
    var result = new Macro(ProgramXml, Program.Settings.MidiForMacros) {
      MacroNo = wheelMacroNo,
      DisplayName = "Wheel",
      Bipolar = false,
      CustomPosition = true,
      IsContinuous = true,
      Value = 0
    };
    result.AddModulation(new Modulation(ProgramXml) {
      CcNo = Program.Settings.MidiForMacros.ModWheelReplacementCcNo
    });
    result.ChangeModWheelModulationSourcesToMacro();
    return result;
  }

  private void OrderMacrosByLocation() {
    var visibleMacrosSortedByLocation = Program.GetMacrosSortedByLocation();
    var newOrder = new List<Macro>();
    newOrder.AddRange(visibleMacrosSortedByLocation);
    var invisibleMacros =
      from macro in Macros
      where !newOrder.Contains(macro)
      select macro;
    newOrder.AddRange(invisibleMacros);
    Macros.Clear();
    Macros.AddRange(newOrder);
    RefreshMacroOrder();
  }

  /// <summary>
  ///   If the macro order has changed, run this to refresh the XML.
  /// </summary>
  public void RefreshMacroOrder() {
    ProgramXml.ReplaceMacroElements(Macros);
    // If we don't reload, relocating the macros jumbles them.
    // Perhaps there's a better way, but it broke when I tried.
    Program.Save();
    Program.Read();
    Program.Log.WriteLine(
      $"{Program.PathShort}: Saved and reloaded on reordering macros.");
  }

  public void RemoveMacros(List<Macro> removableMacros) {
    if (removableMacros.Count == 0) {
      return;
    }
    if (Program.GuiScriptProcessor != null) {
      Program.Log.WriteLine(
        $"{Program.PathShort}: Cannot remove macros because " +
        "because the program's Info page GUI is specified in a script processor.");
      return;
    }
    foreach (var macro in removableMacros) {
      macro.RemoveElement();
      Macros.Remove(macro);
      Program.NotifyUpdate(
        $"{Program.PathShort}: Removed macro {macro}.");
    }
    RefreshMacroOrder();
    MoveMacrosToStandardLayout();
    Program.UpdateMacroCcs();
  }
}