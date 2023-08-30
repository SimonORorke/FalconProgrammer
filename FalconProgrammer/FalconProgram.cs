using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class FalconProgram {
  private const int FirstContinuousCcNo = 31;
  private const int FirstToggleCcNo = 112;
  private InfoPageLayout? _infoPageLayout;

  public FalconProgram(string path, Category category) {
    Path = path;
    // Cannot be read from XML when RestoreOriginal.
    // Trim in case there's a space before the dot in the file name. That would otherwise
    // show up when Name is combined into PathShort.
    Name = System.IO.Path.GetFileNameWithoutExtension(path).TrimEnd();
    Category = category;
  }

  [PublicAPI] public Category Category { get; }

  /// <summary>
  ///   Gets continuous (as opposes to toggle) macros. It's safest to query this each
  ///   time, as it can change already when a delay macro is removed.
  /// </summary>
  internal List<Macro> ContinuousMacros => GetContinuousMacros();

  private ImmutableList<Effect> Effects { get; set; } = null!;
  public bool HasBeenUpdated { get; private set; }

  private InfoPageLayout InfoPageLayout =>
    _infoPageLayout ??= new InfoPageLayout(this);

  private ScriptProcessor? InfoPageCcsScriptProcessor { get; set; }
  internal List<Macro> Macros { get; private set; } = null!;

  /// <summary>
  ///   Gets the order in which MIDI CC numbers are to be mapped to macros
  ///   relative to their locations on the Info page.
  /// </summary>
  private LocationOrder MacroCcLocationOrder { get; set; }

  internal string Name { get; }
  private int NextContinuousCcNo { get; set; } = FirstContinuousCcNo;
  private int NextToggleCcNo { get; set; } = FirstToggleCcNo;
  [PublicAPI] public string Path { get; }

  [PublicAPI]
  public string PathShort =>
    $@"{Category.SoundBankFolder.Name}\{Category.Name}\{Name}";

  internal ProgramXml ProgramXml { get; private set; } = null!;

  private ImmutableList<ScriptProcessor> ScriptProcessors { get; set; } =
    ImmutableList<ScriptProcessor>.Empty;

  private void BypassDelayEffects() {
    foreach (var effect in Effects.Where(effect => effect.IsDelay)) {
      effect.Bypass = true;
      NotifyUpdate($"{PathShort}: Bypassed {effect.EffectType}.");
    }
  }

  private bool CanRemoveInfoPageCcsScriptProcessor() {
    if (Category.InfoPageMustUseScript || Macros.Count > 4) {
      return false;
    }
    // I've customised this script processor and it looks too hard to get rid of.
    // It should be excluded anyway because it has the setting
    // Category.InfoPageMustUseScript true.
    return Category.SoundBankFolder.Name != "Organic Keys";
  }

  /// <summary>
  ///   Returns whether certain conditions are fulfilled indicating that the program's
  ///   mod wheel modulations may be reassigned to a new macro.
  /// </summary>
  private bool CanReplaceModWheelWithMacro() {
    if (WheelMacroExists()) {
      Console.WriteLine(
        $"{PathShort} already has a Wheel macro.");
      return false;
    }
    if (Category.SoundBankFolder.Name == "Ether Fields") {
      // There are lots of macros in every program of this sound bank.
      // I tried adding wheel macros. But it's too busy to be feasible.
      return false;
    }
    if (InfoPageCcsScriptProcessor != null
        && !CanRemoveInfoPageCcsScriptProcessor()) {
      Console.WriteLine(
        $"{PathShort}: Replacing wheel with macro is not supported because " +
        "there is an Info page CCs script processor that is not feasible/desirable " +
        " to remove.");
      return false;
    }
    int modulationsByModWheelCount =
      ProgramXml.GetModulationElementsWithCcNo(1).Count;
    switch (modulationsByModWheelCount) {
      case 0:
        Console.WriteLine($"{PathShort} contains no mod wheel modulations.");
        return false;
      case > 1:
        return true;
    }
    // There is 1 Modulation that has the mod wheel as the modulation source.
    // If the mod wheel only 100% modulates a single macro, there's not point replacing
    // the mod wheel modulations with a Wheel macro.
    // Wheel (MIDI CC number 1) Modulations that modulate a macro are invariably
    // owned by the macro. So, if an InfoPageCcsScriptProcessor exists, we don't need to
    // check the Modulations it owns.
    var macrosOwningModWheelModulations = (
      from macro in Macros
      where macro.FindModulationWithCcNo(1) != null
      select macro).ToList();
    if (macrosOwningModWheelModulations.Count == 1) {
      Console.WriteLine(
        $"{PathShort}: The mod wheel has not been replaced, as it only modulates a " +
        "single macro 100%.");
      // Example: Factory\Pads\DX FM Pad 2.0
      return false;
    }
    return true;
  }

  public void ChangeMacroCcNo(int oldCcNo, int newCcNo) {
    var oldModulation = new Modulation(ProgramXml) { CcNo = oldCcNo };
    var newModulation = new Modulation(ProgramXml) { CcNo = newCcNo };
    ProgramXml.ChangeModulationSource(oldModulation, newModulation);
    NotifyUpdate($"{PathShort}: Changed MIDI CC {oldCcNo}  to {newCcNo}.");
  }

  public void ChangeReverbToZero() {
    // foreach (var effect in Effects.Where(
    //            effect => effect is { IsReverb: true, IsModulated: false })) {
    //   effect.Bypass = true;
    //   Console.WriteLine($"{PathShort}: Bypassed {effect.EffectType}.");
    // }
    var reverbMacros = (
      from macro in Macros
      where macro.ModulatesReverb
      select macro).ToList();
    if (reverbMacros.Count == 0) {
      return;
    }
    if (PathShort is @"Factory\Bass-Sub\Coastal Halftones 1.4"
        or @"Factory\Bass-Sub\Metropolis 1.4"
        or @"Factory\Leads\Ali3n 1.4"
        or @"Factory\Pads\Arrival 1.4"
        // ReSharper disable once StringLiteralTypo
        or @"Factory\Pads\Novachord Noir 1.4"
        or @"Factory\Pads\Pad Motion 1.5"
        or @"Factory\Synth Brass\Gotham Brass 1.4"
        or @"Inner Dimensions\Pad\GrainVoices 2"
        or @"Savage\Pads-Drones\Lunar Nashi"
        or @"Savage\Pads-Drones\Voc Sidechain"
        or @"Savage\Pads-Drones\Wonder Land") {
      // These programs are silent without reverb!
      Console.WriteLine($"Changing reverb to zero is disabled for '{PathShort}'.");
      return;
    }
    foreach (var reverbMacro in reverbMacros) {
      if (reverbMacro.FindModulationWithCcNo(1) != null) {
        // Example: Titanium\Pads\Children's Choir.
        Console.WriteLine(
          $"{PathShort}: Not changing {reverbMacro.DisplayName} to zero because " +
          "it is modulated by the wheel.");
      } else {
        reverbMacro.ChangeValueToZero();
        NotifyUpdate($"{PathShort}: Changed {reverbMacro.DisplayName} to zero.");
      }
    }
  }

  public void CountMacros() {
    if (Macros.Count > 10) {
      Console.WriteLine($"{Macros.Count} macros in '{Path}'.");
    }
  }

  private ProgramXml CreateProgramXml() {
    return Category.InfoPageMustUseScript
      ? new ScriptProgramXml(Category)
      : new ProgramXml(Category);
  }

  /// <summary>
  ///   Finds the ScriptProcessor, if any, that is to contain the Modulations that
  ///   map the macros to MIDI CC numbers. If the ScriptProcessor is not found, each
  ///   macro's MIDI CC number must be defined in a Modulations owned by the
  ///   macro's ConstantModulation.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  private ScriptProcessor? FindInfoPageCcsScriptProcessor() {
    if (ScriptProcessors.Count == 0) {
      return null;
    }
    if (ProgramXml.TemplateScriptProcessorElement == null) {
      if (Category.SoundBankFolder.Name == "Factory") {
        return (
          from scriptProcessor in ScriptProcessors
          // Examples of programs with InfoPageCcsScriptProcessor
          // but no template ScriptProcessor:
          // Factory\Bass-Sub\Balarbas 2.0
          // Factory/Keys/Smooth E-piano 2.1.
          where scriptProcessor.Name == "EventProcessor9"
          select scriptProcessor).FirstOrDefault();
      }
      // Examples of programs with ScriptProcessors but no InfoPageCcsScriptProcessor:
      // Ether Fields\Bells - Plucks\Cloche Esperer
      // Factory\Bass-Sub\BA Shomp 1.2
      return null;
    }
    // Using a template ScriptProcessor
    var matchingScriptProcessor = (
      from scriptProcessor in ScriptProcessors
      // Comparing the Scripts may be more reliable than comparing the Names.
      // Examples where the Scripts match but the Names do not:
      // Inner Dimensions\Pluck\Pulse And Repeat
      // Voklm\Vox Instruments\*
      where scriptProcessor.Script == Category.TemplateScriptProcessor.Script
      select scriptProcessor).FirstOrDefault();
    if (matchingScriptProcessor != null) {
      // Scripts match.
      return matchingScriptProcessor;
    }
    matchingScriptProcessor = (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == Category.TemplateScriptProcessor.Name
      select scriptProcessor).FirstOrDefault();
    if (matchingScriptProcessor != null) {
      // Names match but Scripts do not.
      // Example: Titanium\Basses\Aggression
      return matchingScriptProcessor;
    }
    // Neither Scripts nor Names match. So assume the last ScriptProcessor. 
    // Example: Titanium\Basses\Noiser
    return (
      from scriptProcessor in ScriptProcessors
      select scriptProcessor).Last();
  }

  private Macro? FindReverbContinuousMacro() {
    return (
      from macro in ContinuousMacros
      where macro.ModulatesReverb
      select macro).FirstOrDefault();
  }

  // private Macro? FindReverbToggleMacro() {
  //   return (
  //     from macro in Macros
  //     where macro.ModulatesReverb && !macro.IsContinuous
  //     select macro).FirstOrDefault();
  // }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Macro? FindWheelMacro() {
    return (
      from continuousMacro in ContinuousMacros
      // I changed this to look for DisplayName is 'Wheel' instead of DisplayName
      // contains 'Wheel'. The only two additional programs that subsequently got wheel
      // macros added were
      // Factory\Hybrid Perfs\Louis Funky Dub and Factory\Pluck\Permuda 1.1.
      //
      // In Louis Funky Dub, the original 'Wheel me' macro did and does nothing I can
      // hear. The mod wheel did work, and the added wheel macro has successfully
      // replaced it. So the problem with the 'Wheel me' macro, if it's real, has not
      // been caused by the wheel replacement macro implementation.
      //
      // In Permuda 1.1, the original 'Modwheel' macro controlled the range of the mod
      // wheel and now controls the range of the 'Wheel' macro instead.  So that change
      // has worked too.
      where continuousMacro.DisplayName.ToLower() == "wheel"
      select continuousMacro).FirstOrDefault();
  }

  private int GetCcNo(Macro macro) {
    int result;
    if (macro.IsContinuous) {
      // Map continuous controller CC to continuous macro.
      NextContinuousCcNo = NextContinuousCcNo switch {
        39 => 41,
        49 => 51,
        59 => 61,
        _ => NextContinuousCcNo
      };
      result = NextContinuousCcNo switch {
        // Convert the fifth continuous controller's CC number to 11 to map to the
        // touch strip.
        35 => 11,
        // Convert MIDI CC 38, which does not work with macros on script-based Info
        // pages, to 28.
        38 => 28,
        _ => NextContinuousCcNo
      };
      NextContinuousCcNo++;
    } else {
      // Map button CC to toggle macro. 
      result = NextToggleCcNo;
      NextToggleCcNo++;
    }
    return result;
  }

  private List<Macro> GetContinuousMacros() {
    return (
      from macro in Macros
      where macro.IsContinuous
      select macro).ToList();
  }

  internal SortedSet<Macro> GetMacrosSortedByLocation(
    LocationOrder macroCcLocationOrder) {
    var result = new SortedSet<Macro>(
      macroCcLocationOrder == LocationOrder.TopToBottomLeftToRight
        ? new TopToBottomLeftToRightComparer()
        : new LeftToRightTopToBottomComparer());
    foreach (var macro in Macros) {
      // This validation is not reliable. In "Factory\Bells\Glowing 1.2", the macros with
      // ConstantModulation.Properties showValue="0" are shown on the Info page. 
      //macro.Validate();
      // for (int j = macro.Modulations.Count - 1; j >= 0; j--) {
      //   var modulation = macro.Modulations[j];
      //   // modulation.Index = j;
      // }
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

  private bool HasUniqueLocation(Macro macro) {
    return (
      from m in Macros
      where m.X == macro.X
            && m.Y == macro.Y
      select m).Count() == 1;
  }

  public void MoveConnectionsBeforeProperties() {
    bool updated = false;
    foreach (var dummy in Macros
               .Where(macro => macro.MoveConnectionsBeforeProperties())) {
      updated = true;
    }
    if (updated) {
      NotifyUpdate($"{PathShort}: Moved Connections before Properties.");
    }
  }

  private void MoveMacroToEndIfExists(Macro? macroIfExists) {
    if (macroIfExists != null && macroIfExists != Macros[^1]) {
      Macros.Remove(macroIfExists);
      Macros.Add(macroIfExists);
    }
  }

  private void NotifyUpdate(string message) {
    Console.WriteLine(message);
    HasBeenUpdated = true;
  }

  /// <summary>
  ///   When there are 5 macros, including the wheel macro, and they are all continuous,
  ///   the 4 original macros tend to be more important than the wheel macro, unless
  ///   there's a reverb macro. So, if ther's a reverb macro, move that to the end.
  ///   Otherwise move the wheel macro to the end.  The 4 (main) expression pedals will
  ///   then tend to control the four most important macros.  And once
  ///   <see cref="ReuseCc1" /> has been run, the mod wheel will control the 5th macro
  ///   via MIDI CC 1.
  ///   <para>
  ///     If any of the four original macros are toggles, there's nothing to do,
  ///     as toggle macros are not controlled by expression pedals.
  ///   </para>
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  private void OptimiseWheelMacro() {
    // Examples:
    // Devinity\Plucks-Leads\Pluck Sphere (Reverb was already at end but now all macros
    // will be on one line instead of having the wheel macro above the reverb macro.)
    // Eternal Funk\Brass\Back And Stride (Reverb was previously the first macro. Now
    // it will be last.)
    // Eternal Funk\Synths\Bell Shaka (No reverb. Wheel macro was 4th, now last.)
    if (Macros.Count != 5 || ContinuousMacros.Count != 5) {
      return;
    }
    if (Macros.Any(macro => macro.GetForMacroModulations().Count > 1)) {
      // The problem is modulations by the wheel replacement macro.
      // (Source will indicate the modulating macro,
      // which would be the wheel replacement macro, instead of a MIDI CC number.)
      // Example: "M.1 Cutoff" macro of
      // Hypnotic Drive\Atmospheres & Pads\Vocal Atmos - Prophecy.
      // Requires further investigation. Maybe OK as is.
      return;
    }
    MoveMacroToEndIfExists(FindReverbContinuousMacro());
    MoveMacroToEndIfExists(FindReverbContinuousMacro());
    ProgramXml.ReplaceMacroElements(Macros);
    // If we don't reload, relocating the macros jumbles them.
    // Perhaps there's a better way, but it broke when I tried.
    Save();
    Read();
    Console.WriteLine(
      $"{PathShort}: Saved and reloaded, required for Wheel macro optimisation.");
    InfoPageLayout.MoveAllMacrosToStandardBottom();
    UpdateMacroCcs(LocationOrder.LeftToRightTopToBottom);
    NotifyUpdate($"{PathShort}: Optimised Wheel macro.");
  }

  private void PopulateConnectionsParentsAndEffects() {
    var connectionsParentElements = ProgramXml.GetConnectionsParentElements();
    var effectElements = ProgramXml.GetEffectElements();
    var effects = new List<Effect>();
    foreach (var connectionsParentElement in connectionsParentElements) {
      // ConnectionsParents overlap with Effects. So, if the ConnectionsParent is also an
      // Effect, add it to Effects as well as to ModulatedConnectionsParents of
      // modulating Macros. 
      ConnectionsParent connectionsParent;
      if (effectElements.Contains(connectionsParentElement)) {
        connectionsParent = new Effect(connectionsParentElement, ProgramXml);
        effects.Add((Effect)connectionsParent);
        // Indicate that the Effect has now been added to Effects.
        effectElements.Remove(connectionsParentElement);
      } else {
        connectionsParent = new ConnectionsParent(connectionsParentElement, ProgramXml);
      }
      foreach (var modulation in connectionsParent.Modulations) {
        modulation.SourceMacro = (
          from macro in Macros
          where modulation.Source.EndsWith(macro.Name)
          select macro).FirstOrDefault();
        modulation.SourceMacro?.ModulatedConnectionsParents.Add(connectionsParent);
      }
    }
    // Now add to Effects all remaining Effects, which are those that are not also 
    // ConnectionsParents and so are not modulated by macros or anything else. 
    // ReSharper disable once CommentTypo
    // Example: Titanium\Keys\Synth Xylo 1.
    effects.AddRange(
      from effectElement in effectElements
      select new Effect(effectElement, ProgramXml));
    Effects = effects.ToImmutableList();
  }

  public void PrependPathLineToDescription() {
    const string pathIndicator = "PATH: ";
    const string crLf = "\r\n";
    string oldDescription = ProgramXml.GetDescription();
    string oldPathLine =
      oldDescription.StartsWith(pathIndicator) && oldDescription.Contains(crLf)
        ? oldDescription[..(oldDescription.IndexOf(crLf, StringComparison.Ordinal) + 2)]
        : string.Empty;
    string newPathLine = pathIndicator + PathShort + crLf;
    string newDescription = oldPathLine != string.Empty
      ? oldDescription.Replace(oldPathLine, newPathLine)
      : newPathLine + oldDescription;
    ProgramXml.SetDescription(newDescription);
    NotifyUpdate($"{PathShort}: Prepended path line to description.");
  }

  public IEnumerable<string> QueryDelayTypes() {
    var result = new List<string>();
    foreach (var macro in Macros.Where(macro => macro.ModulatesDelay)) {
      foreach (var connectionsParent in macro.ModulatedConnectionsParents.Where(
                 connectionsParent =>
                   connectionsParent is Effect effect &&
                   !result.Contains(effect.EffectType))) {
        result.Add(((Effect)connectionsParent).EffectType);
      }
    }
    return result;
  }

  public IEnumerable<string> QueryReverbTypes() {
    var result = new List<string>();
    foreach (var macro in Macros.Where(macro => macro.ModulatesReverb)) {
      foreach (var connectionsParent in macro.ModulatedConnectionsParents.Where(
                 connectionsParent =>
                   connectionsParent is Effect effect &&
                   !result.Contains(effect.EffectType))) {
        result.Add(((Effect)connectionsParent).EffectType);
      }
    }
    return result;
  }

  public void QueryReuseCc1NotSupported() {
    if (InfoPageCcsScriptProcessor == null
        || ContinuousMacros.Count < 5
        || ProgramXml.GetModulationElementsWithCcNo(1).Count > 0) {
      return;
    }
    Console.WriteLine($"{PathShort}: Reusing MIDI CC 1 is not yet supported.");
  }

  public void Read() {
    ProgramXml = CreateProgramXml();
    ProgramXml.LoadFromFile(Path);
    Category.ProgramXml = ProgramXml;
    Macros = (
      from macroElement in ProgramXml.MacroElements
      select new Macro(macroElement, ProgramXml)).ToList();
    foreach (var macro in Macros) {
      foreach (var modulation in macro.Modulations) {
        modulation.Owner = macro;
      }
    }
    ScriptProcessors = (
      from scriptProcessorElement in ProgramXml.ScriptProcessorElements
      select new ScriptProcessor(scriptProcessorElement, ProgramXml)).ToImmutableList();
    foreach (var scriptProcessor in ScriptProcessors) {
      foreach (var modulation in scriptProcessor.Modulations) {
        // Needed for modulation.ModulatesMacro in FindInfoPageCcsScriptProcessor 
        modulation.Owner = scriptProcessor;
      }
    }
    InfoPageCcsScriptProcessor = FindInfoPageCcsScriptProcessor();
    PopulateConnectionsParentsAndEffects();
  }

  public void RemoveDelayEffectsAndMacros() {
    BypassDelayEffects();
    if (RemoveDelayMacros()) {
      if (InfoPageCcsScriptProcessor != null) {
        RemoveInfoPageCcsScriptProcessor();
      } else {
        ReUpdateMacroCcs();
      }
    }
  }

  /// <summary>
  ///   Removes delay macros, if any.
  ///   These are recognised by the macro name, or if all the effects the
  ///   macro modulates have been bypassed by <see cref="BypassDelayEffects" />.
  /// </summary>
  private bool RemoveDelayMacros() {
    var removableMacros = (
      from macro in Macros
      where macro.ModulatesDelay || !macro.ModulatesEnabledEffects
      select macro).ToList();
    if (removableMacros.Count == 0) {
      return false;
    }
    if (InfoPageCcsScriptProcessor != null
        && !CanRemoveInfoPageCcsScriptProcessor()) {
      Console.WriteLine(
        $"{PathShort}: Cannot remove macros because " +
        "there is an Info page CCs script processor that is not feasible/desirable " +
        " to remove.");
      return false;
    }
    foreach (var macro in removableMacros) {
      macro.RemoveElement();
      Macros.Remove(macro);
      NotifyUpdate($"{PathShort}: Removed {macro}.");
    }
    return true;
  }

  /// <summary>
  ///   Do away with the <see cref="InfoPageCcsScriptProcessor" />, which defines a
  ///   special Info Page layout and MIDI CC numbers that modulate macros.
  ///   That will give the Info page the default appearance and allow macros to be moved,
  ///   added and removed. When the script processor has been dispensed with,
  ///   move the existing macros to locations optimal for accommodating a new wheel
  ///   replacement macro.
  /// </summary>
  private void RemoveInfoPageCcsScriptProcessor() {
    // ProgramXml.RemoveInfoPageCcsScriptProcessorElement();
    InfoPageCcsScriptProcessor!.Remove();
    InfoPageCcsScriptProcessor = null;
    ReUpdateMacroCcs();
    foreach (var macro in Macros) {
      macro.CustomPosition = true;
      // As we are going to convert script processor-owned 'for macro' (i.e. as opposed to
      // for the mod wheel) Modulations to macro-owned 'for macro' Modulations,
      // there should not be any existing macro-owned 'for macro' Modulations.
      // If there are, get rid of them.
      // Example: Titanium\Pads\Children's Choir.
      var forMacroModulations =
        macro.GetForMacroModulations();
      foreach (var forMacroModulation in forMacroModulations) {
        macro.RemoveModulation(forMacroModulation);
      }
    }
    Console.WriteLine($"{PathShort}: Removed Info Page CCs ScriptProcessor.");
    InfoPageLayout.MoveAllMacrosToStandardBottom();
    ReUpdateMacroCcs();
  }

  /// <summary>
  ///   If feasible, replaces all modulations by the modulation wheel of effect
  ///   parameters with modulations by a new 'Wheel' macro. Otherwise shows a message
  ///   explaining why it is not feasible.
  /// </summary>
  public void ReplaceModWheelWithMacro() {
    if (!CanReplaceModWheelWithMacro()) {
      return;
    }
    if (InfoPageCcsScriptProcessor != null) {
      RemoveInfoPageCcsScriptProcessor();
    }
    if (InfoPageLayout.TryReplaceModWheelWithMacro(
          out bool updateMacroCcs)) {
      if (updateMacroCcs) {
        // Saving and reading prevents wheel macro duplicate modulation error.
        // Example: Devinity\Bass\Comber Bass.
        // There has to be a better way.
        Save();
        Read();
        Console.WriteLine(
          $"{PathShort}: Saved and reloaded, required for Wheel macro CCs update.");
        ReUpdateMacroCcs();
      }
      NotifyUpdate($"{PathShort}: Replaced mod wheel with macro.");
      OptimiseWheelMacro();
    } else {
      throw new InvalidOperationException(
        $"{PathShort}: Failed to replace mod wheel with macro.");
    }
  }

  private void ReUpdateMacroCcs() {
    // This should be the default, but otherwise won't work with wheel replacement. 
    MacroCcLocationOrder = LocationOrder.LeftToRightTopToBottom;
    UpdateMacroCcsOwnedByMacros();
    Console.WriteLine($"{PathShort}: Re-updated macro Ccs.");
  }

  public void RestoreOriginal() {
    string originalPath = System.IO.Path.Combine(
      System.IO.Path.GetDirectoryName(Path)!.Replace(
        "FalconPrograms", "Programs ORIGINAL") + " ORIGINAL",
      System.IO.Path.GetFileName(Path));
    if (!File.Exists(originalPath)) {
      Console.WriteLine($"Cannot find original file '{originalPath}' for '{Path}'.");
      return;
    }
    File.Copy(originalPath, Path, true);
    Console.WriteLine($"{PathShort}: Restored to Original");
  }

  /// <summary>
  ///   Assuming any modulations by the wheel macro have already been reassigned to a
  ///   wheel macro, if there are at least 5 continuous macros, i.e. at least 1 more than
  ///   the usual number of expression pedals that can control them, make the best use
  ///   of the keyboard's hardware controllers by assigning MIDI CC 1 (mod wheel) to the
  ///   5th continuous macro and MIDI CC 11 (touch strip) to the 6th, if there is one.
  ///   Increment MIDI CCs of any subsequent macros accordingly.
  ///   <para>
  ///     For programs with an Info page MIDI CCs script processor,
  ///     changing the MIDI CCs modulating macros is not (yet) supported by this method.
  ///     If it were to be implemented, 51 programs would be impacted, at last count by
  ///     <see cref="QueryReuseCc1NotSupported" />: 32 in Fluidity and in 19 Pulsar.
  ///   </para>
  /// </summary>
  public void ReuseCc1() {
    if (InfoPageCcsScriptProcessor != null // See paragraph in summary.
        || ContinuousMacros.Count < 5
        || (ProgramXml.GetModulationElementsWithCcNo(1)
              .Count > 0
            && !WheelMacroExists())) {
      return;
    }
    var continuousMacrosByLocation = (
      from continuousMacro in GetMacrosSortedByLocation(MacroCcLocationOrder)
      where continuousMacro.IsContinuous
      select continuousMacro).ToList();
    if (continuousMacrosByLocation.Count < 5) {
      // Allow for macros with invalid locations: see GetMacrosSortedByLocation. 
      return;
    }
    continuousMacrosByLocation[4].ChangeCcNoTo(1);
    if (continuousMacrosByLocation.Count > 5) {
      continuousMacrosByLocation[5].ChangeCcNoTo(11);
    }
    if (continuousMacrosByLocation.Count > 6) {
      // Example with many continuous macros: Ether Fields\Bells - Plucks\FM Harp
      int newCcNo = 36;
      for (int i = 6; i < continuousMacrosByLocation.Count; i++) {
        continuousMacrosByLocation[i].ChangeCcNoTo(newCcNo);
        newCcNo++;
      }
    }
    NotifyUpdate($"{PathShort}: Reused MIDI CC 1.");
  }

  public void Save() {
    ProgramXml.SaveToFile(Path);
  }

  public void UpdateMacroCcs(LocationOrder macroCcLocationOrder) {
    MacroCcLocationOrder = macroCcLocationOrder;
    if (InfoPageCcsScriptProcessor == null) {
      // The CCs are specified in Modulations owned by the Macros
      // (ConstantModulations) that they modulate
      UpdateMacroCcsOwnedByMacros();
    } else if (Category.TemplateScriptProcessor != null) {
      // The CCs are specified Modulations owned by the Info page ScriptProcessor
      // and can be copied from a template ScriptProcessor.
      // This applies to all programs in categories for which InfoPageMustUseScript
      // is set to true in the settings file.
      // In some categories, we have or are going to remove the Info page
      // ScriptProcessor, so InfoPageMustUseScript has had to be changed to false for
      // the Category, yet we still need to use the template if it is available.
      InfoPageCcsScriptProcessor.UpdateModulationsFromTemplate(
        Category.TemplateScriptProcessor.Modulations);
    } else {
      // The CCs are specified in the Info page ScriptProcessor but there's no template
      // ScriptProcessor. 
      UpdateMacroCcsOwnedByScriptProcessor();
    }
    NotifyUpdate($"{PathShort}: Updated Macro CCs.");
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in
  ///   ConstantModulations,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and toggle macros.
  /// </summary>
  private void UpdateMacroCcsOwnedByMacros() {
    // Most Factory programs list the ConstantModulation macro specifications in order
    // top to bottom, left to right. But a few, e.g. Factory/Keys/Days Of Old 1.4, do not.
    //
    var sortedByLocation = GetMacrosSortedByLocation(MacroCcLocationOrder);
    // Reinitialise NextContinuousCcNo, incremented by GetCcNo, in case
    // UpdateMacroCcsInConstantModulations is called multiple times. It is called twice
    // by RemoveInfoPageCcsScriptProcessor, the second time via
    // ReplaceModWheelWithMacro. 
    NextContinuousCcNo = FirstContinuousCcNo;
    NextToggleCcNo = FirstToggleCcNo;
    foreach (var macro in sortedByLocation) {
      // Retain unaltered any mapping to the modulation wheel (MIDI CC 1) or any other
      // MIDI CC mapping that's not on the Info page.
      // Example: Devinity/Bass/Comber Bass.
      var forMacroModulations =
        macro.GetForMacroModulations();
      if (forMacroModulations.Count > 1) {
        throw new NotSupportedException(
          $"{PathShort}: Macro '{macro}' owns {forMacroModulations.Count} " +
          "'for macro' Modulations.");
      }
      // if (forMacroModulations.Count !=
      //     macro.Modulations.Count) {
      //   Console.WriteLine(
      //     $"{PathShort}: Modulation wheel assignment found in macro {macro}.");
      // }
      int ccNo = GetCcNo(macro);
      if (forMacroModulations.Count == 0) { // Will be 0 or 1
        // The macro is not already mapped to a non-mod wheel CC number.
        var modulation = new Modulation(ProgramXml) {
          CcNo = ccNo
        };
        macro.AddModulation(modulation);
      } else {
        // The macro already has a Modulation mapping to a non-mod wheel CC number.
        // We need to conserve the Modulation tag, which might contain a custom
        // Ratio, and, with the exception below, just replace the CC number.
        var modulation = forMacroModulations[0];
        modulation.CcNo = ccNo;
        // In Factory/Keys/Days Of Old 1.4, Macro 1, a toggle macro, has Ratio -1 instead
        // of the usual 1. I don't know what the point of that is. But it prevents the
        // button controller mapped to the macro from working. To fix this, if a toggle
        // macro has Ratio -1, update Ratio to 1. I cannot see any disadvantage in doing
        // that. 
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (!macro.IsContinuous && modulation.Ratio == -1) {
          modulation.Ratio = 1;
        }
      }
    }
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in a
  ///   ScriptProcessor,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and toggle macros.
  ///   All applicable programs are in Factory, identified by "EventProcessor9" being the
  ///   InfoPageCcsScriptProcessor: see <see cref="FindInfoPageCcsScriptProcessor" />.
  ///   Examples:
  ///   Factory\Bass-Sub\Balarbas 2.0
  ///   Factory\Keys\Smooth E-piano 2.1.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  private void UpdateMacroCcsOwnedByScriptProcessor() {
    var sortedByLocation =
      GetMacrosSortedByLocation(MacroCcLocationOrder);
    int macroNo = 0;
    for (int i = InfoPageCcsScriptProcessor!.Modulations.Count - 1; i >= 0; i--) {
      var modulation = InfoPageCcsScriptProcessor!.Modulations[i];
      if (modulation.ModulatesMacro) {
        InfoPageCcsScriptProcessor!.RemoveModulation(modulation);
      }
    }
    NextContinuousCcNo = FirstContinuousCcNo;
    NextToggleCcNo = FirstToggleCcNo;
    foreach (var macro in sortedByLocation) {
      macroNo++; 
      InfoPageCcsScriptProcessor.AddModulation(
        new Modulation(ProgramXml) {
          Destination = $"Macro{macroNo}",
          CcNo = GetCcNo(macro)
        });
    }
  }

  private bool WheelMacroExists() {
    return FindWheelMacro() != null;
  }
}