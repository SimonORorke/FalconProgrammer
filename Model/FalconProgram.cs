using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model;

public class FalconProgram {
  private InfoPageLayout? _infoPageLayout;

  public FalconProgram(string path, Category category, Batch batch) {
    Path = path;
    Name = System.IO.Path.GetFileNameWithoutExtension(path).Trim();
    Category = category;
    Batch = batch;
  }

  private Batch Batch { get; }
  public Category Category { get; }

  /// <summary>
  ///   Gets continuous (as opposes to toggle) macros. It's safest to query this each
  ///   time, as it can change already when a delay macro is removed.
  /// </summary>
  internal List<Macro> ContinuousMacros => GetContinuousMacros();

  private ImmutableList<Effect> Effects { get; set; } = null!;
  private ScriptProcessor? GuiScriptProcessor { get; set; }
  public bool HasBeenUpdated { get; private set; }

  private InfoPageLayout InfoPageLayout =>
    _infoPageLayout ??= new InfoPageLayout(this);

  private IBatchLog Log => Batch.Log;
  internal List<Macro> Macros { get; private set; } = null!;

  /// <summary>
  ///   Gets the order in which MIDI CC numbers are to be mapped to macros
  ///   relative to their locations on the Info page.
  /// </summary>
  public LocationOrder MacroCcLocationOrder =>
    GuiScriptProcessor != null
      ? LocationOrder.LeftToRightTopToBottom
      : LocationOrder.TopToBottomLeftToRight;

  /// <summary>
  ///   Cannot be read from XML when RestoreOriginal.
  ///   Trim in case there's a space before the dot in the file name. That would otherwise
  ///   show up when Name is combined into PathShort.
  /// </summary>
  private string Name { get; }

  private int CurrentContinuousCcNo { get; set; }
  private int CurrentToggleCcNo { get; set; }
  [PublicAPI] public string Path { get; }

  [PublicAPI]
  public string PathShort =>
    $@"{SoundBankName}\{Category.Name}\{Name}";

  internal ProgramXml ProgramXml { get; private set; } = null!;

  private ImmutableList<ScriptProcessor> ScriptProcessors { get; set; } =
    ImmutableList<ScriptProcessor>.Empty;

  public Settings Settings => Batch.Settings;
  private string SoundBankName => Category.SoundBankName;

  private void BypassDelayEffects() {
    foreach (var effect in Effects.Where(effect => effect.IsDelay)) {
      effect.Bypass = true;
      NotifyUpdate($"{PathShort}: Bypassed {effect.EffectType}.");
    }
  }

  private void BypassReverbEffects() {
    foreach (var effect in Effects.Where(effect => effect.IsReverb)) {
      effect.Bypass = true;
      NotifyUpdate($"{PathShort}: Bypassed {effect.EffectType}.");
    }
  }

  private bool CanRemoveGuiScriptProcessor() {
    if (Category.MustUseGuiScriptProcessor || Macros.Count > 4) {
      return false;
    }
    // I've customised this script processor and it looks too hard to get rid of.
    // It should be excluded anyway because it has the setting
    // Category.MustUseGuiScriptProcessor true.
    return SoundBankName != "Organic Keys";
  }

  /// <summary>
  ///   Returns whether certain conditions are fulfilled indicating that the program's
  ///   mod wheel modulations may be reassigned to a new macro.
  /// </summary>
  private bool CanReplaceModWheelWithMacro() {
    if (WheelMacroExists()) {
      Log.WriteLine(
        $"{PathShort} already has a Wheel macro.");
      return false;
    }
    if (SoundBankName == "Ether Fields") {
      // There are lots of macros in every program of this sound bank.
      // I tried adding wheel macros. But it's too busy to be feasible.
      return false;
    }
    if (GuiScriptProcessor != null
        && !CanRemoveGuiScriptProcessor()) {
      Log.WriteLine(
        $"{PathShort}: Replacing wheel with macro is not supported because " +
        "there is a GUI script processor that is not feasible/desirable " +
        "to remove.");
      return false;
    }
    int modulationsByModWheelCount =
      ProgramXml.GetModulationElementsWithCcNo(1).Count;
    switch (modulationsByModWheelCount) {
      case 0:
        Log.WriteLine($"{PathShort} contains no mod wheel modulations.");
        return false;
      case > 1:
        return true;
    }
    // There is 1 Modulation that has the mod wheel as the modulation source.
    // If the mod wheel only 100% modulates a single macro, there's not point replacing
    // the mod wheel modulations with a Wheel macro.
    // Wheel (MIDI CC number 1) Modulations that modulate a macro are invariably
    // owned by the macro. So, if an GuiScriptProcessor exists, we don't need to
    // check the Modulations it owns.
    var macrosOwningModWheelModulations = (
      from macro in Macros
      where macro.IsModulatedByWheel
      select macro).ToList();
    if (macrosOwningModWheelModulations.Count == 1) {
      Log.WriteLine(
        $"{PathShort}: The mod wheel has not been replaced, as it only modulates a " +
        "single macro 100%.");
      // Example: Factory\Pads\DX FM Pad 2.0
      return false;
    }
    return true;
  }

  [ExcludeFromCodeCoverage]
  protected virtual void CopyFile(string sourcePath, string destinationPath) {
    File.Copy(sourcePath, destinationPath, true);
  }

  private List<Macro> CreateMacrosFromElements() {
    var result = (
      from macroElement in ProgramXml.MacroElements
      select new Macro(macroElement, ProgramXml, Settings.MidiForMacros)).ToList();
    foreach (var macro in result) {
      foreach (var modulation in macro.Modulations) {
        modulation.Owner = macro;
      }
    }
    return result;
  }

  /// <summary>
  ///   Only used for Organic Pads sound bank in <see cref="FixCData" />.
  /// </summary>
  [ExcludeFromCodeCoverage]
  protected virtual TextReader CreateProgramReader() {
    return new StreamReader(Path);
  }

  protected virtual ProgramXml CreateProgramXml() {
    return Category.MustUseGuiScriptProcessor
      ? new ScriptProgramXml(Category)
      : new ProgramXml(Category);
  }

  private Macro? FindContinuousMacro(string displayName) {
    return (
      from macro in ContinuousMacros
      where macro.DisplayName == displayName
      select macro).FirstOrDefault();
  }

  /// <summary>
  ///   Finds the ScriptProcessor, if any, that is to contain the Modulations that
  ///   map the macros to MIDI CC numbers. If the ScriptProcessor is not found, each
  ///   macro's MIDI CC number must be defined in a Modulations owned by the
  ///   macro's ConstantModulation.
  /// </summary>
  [SuppressMessage("ReSharper", "CommentTypo")]
  private ScriptProcessor? FindGuiScriptProcessor() {
    if (ScriptProcessors.Count == 0) {
      return null;
    }
    if (ProgramXml.TemplateScriptProcessorElement == null) {
      if (SoundBankName.StartsWith("Factory")) {
        return (
          from scriptProcessor in ScriptProcessors
          // Examples of programs with GuiScriptProcessor
          // but no template ScriptProcessor:
          // Factory\Bass-Sub\Balarbas 2.0
          // Factory\Keys\Smooth E-piano 2.1.
          where scriptProcessor.Name == "EventProcessor9"
          select scriptProcessor).FirstOrDefault();
      }
      // Examples of programs with ScriptProcessors but no GuiScriptProcessor:
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
      where scriptProcessor.Script == Category.TemplateScriptProcessor!.Script
      select scriptProcessor).FirstOrDefault();
    if (matchingScriptProcessor != null) {
      // Scripts match.
      return matchingScriptProcessor;
    }
    matchingScriptProcessor = (
      from scriptProcessor in ScriptProcessors
      where scriptProcessor.Name == Category.TemplateScriptProcessor!.Name
      select scriptProcessor).FirstOrDefault();
    if (matchingScriptProcessor != null) {
      // Names match but Scripts do not.
      // Example: Titanium\Basses\Aggression
      if (SoundBankName == "Organic Pads"
          && matchingScriptProcessor.Script.Contains("DahdsrController")) {
        // This is not a GUI script processor but the special script processor added in
        // InitialiseLayout.
        return null;
      }
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

  private Macro? FindReverbToggleMacro() {
    return (
      from macro in Macros
      where !macro.IsContinuous && macro.ModulatesReverb
      select macro).FirstOrDefault();
  }

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
      where continuousMacro.DisplayName.Equals(
        "wheel", StringComparison.CurrentCultureIgnoreCase)
      select continuousMacro).FirstOrDefault();
  }

  /// <summary>
  ///   If the InitialiseLayout batch task has just run for the "Organic Pads"
  ///   sound bank, the chevron delimiters of the DAHDSR Controller script CDATA will
  ///   have been incorrectly written as their corresponding HTML substitutes.
  ///   To fix that, the batch needs to call this method after the Save for
  ///   InitialiseLayout.
  /// </summary>
  public void FixCData() {
    if (SoundBankName != "Organic Pads") {
      return;
    }
    var reader = CreateProgramReader();
    string oldContents = reader.ReadToEnd();
    reader.Close();
    string newContents = oldContents
      .Replace("<script>&lt;", "<script><")
      .Replace("&gt;</script>", "></script>");
    UpdateProgramFileWithFixedCData(newContents);
  }

  /// <summary>
  ///   Returns a dictionary of macros for the standard envelope parameters
  ///   Attack, Decay, Sustain and Release.
  /// </summary>
  /// <remarks>
  ///   Examples where there are all four:
  ///   many Eternal Funk programs; Ether Fields\Hybrid\Cine Guitar Pad.
  /// </remarks>
  internal Dictionary<string, Macro> GetAdsrMacros() {
    string[] displayNames = ["Attack", "Decay", "Sustain", "Release"];
    var result = new Dictionary<string, Macro>();
    foreach (string displayName in displayNames) {
      var macro = FindContinuousMacro(displayName);
      if (macro != null) {
        result.Add(displayName, macro);
      }
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
    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
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

  private int GetNextCcNo(Macro macro, bool reuseCc1) {
    int currentContinuousCcNo = CurrentContinuousCcNo;
    int currentToggleCcNo = CurrentToggleCcNo;
    int ccNo = macro.GetNextCcNo(
      ref currentContinuousCcNo, ref currentToggleCcNo, reuseCc1);
    CurrentContinuousCcNo = currentContinuousCcNo;
    CurrentToggleCcNo = currentToggleCcNo;
    return ccNo;
  }

  private bool HasUniqueLocation(Macro macro) {
    return (
      from m in Macros
      where m.X == macro.X
            && m.Y == macro.Y
      select m).Count() == 1;
  }

  public void InitialiseLayout() {
    PrependPathLineToDescription();
    // There can be a delay loading programs with GUI script processors, for example
    // nearly 10 seconds for Modular Noise\Keys\Inscriptions. So remove the GUI script
    // processor, if there is one, unless there is a need to keep it.
    // This saves having to decide later whether to remove the GUI script processor,
    // such as to add a wheel macro or remove a delay macro.
    if (GuiScriptProcessor != null && !Category.MustUseGuiScriptProcessor) {
      RemoveGuiScriptProcessor();
    }
    if (GuiScriptProcessor != null) {
      return;
    }
    if (Settings.TryGetSoundBankBackgroundImagePath(
          SoundBankName, out string path)) {
      SetBackgroundImagePath();
    }
    switch (SoundBankName) {
      case "Ether Fields" or "Spectre":
        InfoPageLayout.MoveMacrosToStandardLayout();
        break;
      case "Fluidity":
        var attackMacro = FindContinuousMacro("Attack");
        if (attackMacro != null) {
          MoveMacroToEnd(attackMacro);
          RefreshMacroOrder();
        }
        break;
      case "Organic Pads":
        InitialiseOrganicPadsProgram();
        break;
      default:
        return;
    }
    // NotifyUpdate($"{PathShort}: Initialised layout.");
    return;

    void SetBackgroundImagePath() {
      string relativePath =
        System.IO.Path.GetRelativePath(Category.Path, path);
      string falconFormatPath = "./" + relativePath.Replace(@"\", "/");
      ProgramXml.SetBackgroundImagePath(falconFormatPath);
      NotifyUpdate($"{PathShort}: Set BackgroundImagePath.");
    }
  }

  private void InitialiseOrganicPadsProgram() {
    ProgramXml.CopyMacroElementsFromTemplate();
    Macros = CreateMacrosFromElements();
    NotifyUpdate($"{PathShort}: Copied macros from template.");
    // The Wheel macro is Macro 9.
    // So all modulations whose source is the original Wheel macro, Macro 1,
    // need to be updated to Macro 9 instead.
    ProgramXml.ChangeModulationSource(
      "$Program/Macro 1", "$Program/Macro 9");
    NotifyUpdate($"{PathShort}: Updated modulations by Wheel macro.");
    // In the original program, the four main timbre parameters (Synthesis, Sample, Noise
    // and Texture) are controlled by an XY control in the script GUI.
    // In this customised program, to make variation of these parameters mutually
    // independent, and to take full advantage of my four main expression pedal
    // controllers, each is controlled by a separate macro.
    // So make each of Macros 1 to 4 control the Gain property of the Layer with the
    // DisplayName matching the macro's DisplayName. And initialise the macros to the
    // corresponding Layer Gains.
    var layers = ProgramXml.GetLayers();
    foreach (var layer in layers) {
      var sourceMacro = FindContinuousMacro(layer.DisplayName);
      if (sourceMacro == null) {
        throw new NotSupportedException(
          $"{layer.DisplayName} Layer is not supported.");
      }
      sourceMacro.Value = layer.Gain;
      var modulation = new Modulation(ProgramXml) {
        Destination = "Gain",
        Owner = layer,
        Source = $"$Program/{sourceMacro.Name}",
        SourceMacro = sourceMacro
      };
      layer.AddModulation(modulation);
    }
    NotifyUpdate($"{PathShort}: " +
                 "Added modulations to layers and initialised macros to layer gains.");
    // Add the DAHDSR Controller ScriptProcessor.
    var scriptProcessor = ProgramXml.AddScriptProcessor(
      "EventProcessor0", "Organic Pads",
      "./../../../Scripts/DAHDSR Controller.lua",
      "<![CDATA[require 'DahdsrController/DahdsrController']]>");
    NotifyUpdate($"{PathShort}: Added ScriptProcessor.");
    // Add the ScriptProcessor modulations.
    var adsrMacros = GetAdsrMacros().Values;
    foreach (var adsrMacro in adsrMacros) {
      scriptProcessor.AddModulation(new Modulation(ProgramXml) {
        Destination = adsrMacro.DisplayName,
        Owner = scriptProcessor,
        Source = $"$Program/{adsrMacro.Name}",
        SourceMacro = adsrMacro
      });
    }
    NotifyUpdate($"{PathShort}: Added ScriptProcessor Modulations.");
    // Initialise the DAHDSR attack and release times to subvert the original intention 
    // for the sound to be a pad!
    // A problem still to solve is that reducing the attack time to zero while playing
    // can cause low volume and then silence. See the comment at the top of the
    // DahdsrController script for details.
    // So, for safety, that's another reason to initialise the attack time to be fast.
    var mainDahdsr = ProgramXml.FindMainDahdsr();
    if (mainDahdsr == null) {
      throw new InvalidOperationException(
        $"{PathShort}: Cannot find DAHDSR in ControlSignalSources.");
    }
    mainDahdsr.AttackTime = 0.02f;
    mainDahdsr.ReleaseTime = 0.3f;
    NotifyUpdate(
      $"{PathShort}: Initialised '{mainDahdsr.DisplayName}'.AttackTime " +
      "and .ReleaseTime.");
    BypassReverbEffects();
  }

  /// <summary>
  ///   Moves release and reverb macros with zero values to the end of the standard GUI
  ///   layout. Run after <see cref="ZeroReleaseMacro" /> and
  ///   <see cref="ZeroReverbMacros" />.
  ///   If we only refrained from zeroing a macro because it is modulated by the wheel,
  ///   we will assume that the player will use the wheel, and so the macro will still be
  ///   moved to the end.
  /// </summary>
  public void MoveZeroedMacrosToEnd() {
    if (GuiScriptProcessor != null) {
      Log.WriteLine(
        $"{PathShort}: Cannot move macros because " +
        "there is a GUI script processor that is not feasible/desirable " +
        "to remove.");
      return;
    }
    var macrosToMove = new List<Macro>();
    if (TryGetNonAdsrReleaseMacro(out var releaseMacro)
        && (releaseMacro!.Value == 0 || releaseMacro.IsModulatedByWheel)) {
      macrosToMove.Add(releaseMacro);
    }
    var reverbToggleMacro = FindReverbToggleMacro();
    if (reverbToggleMacro?.Value == 0) {
      macrosToMove.Add(reverbToggleMacro);
    }
    var reverbContinuousMacro = FindReverbContinuousMacro();
    if (reverbContinuousMacro != null) {
      if (reverbContinuousMacro.Value == 0
          // If we only refrained from zeroing the macro because it's modulated by the
          // wheel, assume the player will use the wheel, and so still move it to the
          // end.
          // Example: Titanium\Pads\Children's Choir.
          || (reverbContinuousMacro.IsModulatedByWheel &&
              Settings.CanChangeReverbToZero(
                SoundBankName, Category.Name, Name))) {
        macrosToMove.Add(reverbContinuousMacro);
      }
    }
    if (macrosToMove.Count > 0) {
      foreach (var macro in macrosToMove) {
        MoveMacroToEnd(macro);
      }
      RefreshMacroOrder();
      InfoPageLayout.MoveMacrosToStandardLayout();
      ReUpdateMacroCcs();
      NotifyUpdate($"{PathShort}: Moved zeroed macros to end.");
    }
  }

  /// <summary>
  ///   Moves the specified macros to the end of the layout of macros on the Info page.
  /// </summary>
  /// <remarks>
  ///   After one or more calls to <see cref="MoveMacroToEnd" />,
  ///   <see cref="RefreshMacroOrder" /> must be called.
  /// </remarks>
  private void MoveMacroToEnd(Macro macro) {
    if (macro != Macros[^1]) {
      Macros.Remove(macro);
      Macros.Add(macro);
      NotifyUpdate($"{PathShort}: Moved {macro.DisplayName} macro to end.");
    }
  }

  public void NotifyUpdate(string message) {
    Log.WriteLine(message);
    HasBeenUpdated = true;
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

  /// <summary>
  ///   Prepends a line indicating the program's short path to the program's description,
  ///   which is viewable in Falcon when the Info page's  "i" button is clicked.
  /// </summary>
  /// <remarks>
  ///   This has the unwanted effect of replacing the line breaks in the description with
  ///   spaces. See the comment in <see cref="ProgramXml.ReadRootElementFromXmlText" />.
  ///   I tried a workaround by updating the description by parsing plain text updates
  ///   rather than via Linq to XML. But updates by later tasks caused further
  ///   description changes and somehow messed up wheel macro changes.
  /// </remarks>
  private void PrependPathLineToDescription() {
    const string pathIndicator = "PATH: ";
    const string crLf = "\r\n";
    ProgramXml.InitialiseDescription();
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

  public void QueryAdsrMacros() {
    if (GuiScriptProcessor != null) {
      return;
    }
    int count = (
      from macro in ContinuousMacros
      where macro.DisplayName is "Attack" or "Decay" or "Sustain" or "Release"
      select macro).Count();
    if (count == 4) {
      Log.WriteLine($"{PathShort} has ADSR macros.");
    }
  }

  /// <summary>
  ///   Reports how many macros the program has.
  /// </summary>
  public void QueryCountMacros() {
    Log.WriteLine($"{PathShort} has {Macros.Count} macros.");
  }

  public void QueryDahdsrModulations() {
    if (GuiScriptProcessor != null) {
      return;
    }
    var dahdsrs = ProgramXml.GetDahdsrs();
    foreach (var dahdsr in dahdsrs.Where(dahdsr => dahdsr.Modulations.Count > 1)) {
      using var writer = new StringWriter();
      writer.WriteAsync(
        $"{PathShort}: {dahdsr.DisplayName} has " +
        $"{dahdsr.Modulations.Count} modulations: ");
      foreach (var modulation in dahdsr.Modulations) {
        writer.WriteAsync($"{modulation.Destination} ");
      }
      Log.WriteLine(writer.ToString());
    }
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

  public void QueryMainDahdsr() {
    // if (GuiScriptProcessor != null) {
    //   return;
    // }
    var mainDahdsr = ProgramXml.FindMainDahdsr();
    if (mainDahdsr != null) {
      Log.WriteLine($"{PathShort}: {mainDahdsr.DisplayName}");
    }
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
    if (GuiScriptProcessor == null
        || ContinuousMacros.Count < 5
        || ProgramXml.GetModulationElementsWithCcNo(1).Count > 0) {
      return;
    }
    Log.WriteLine($"{PathShort}: Reusing MIDI CC 1 is not yet supported.");
  }

  public void Read() {
    ProgramXml = CreateProgramXml();
    ProgramXml.LoadFromFile(Path);
    Category.ProgramXml = ProgramXml;
    Macros = CreateMacrosFromElements();
    ScriptProcessors = (
      from scriptProcessorElement in ProgramXml.ScriptProcessorElements
      select ScriptProcessor.Create(
        SoundBankName, scriptProcessorElement, ProgramXml)).ToImmutableList();
    foreach (var scriptProcessor in ScriptProcessors) {
      foreach (var modulation in scriptProcessor.Modulations) {
        // Needed for modulation.ModulatesMacro in FindGuiScriptProcessor 
        modulation.Owner = scriptProcessor;
      }
    }
    GuiScriptProcessor = FindGuiScriptProcessor();
    PopulateConnectionsParentsAndEffects();
  }

  /// <summary>
  ///   If the macro order has changed, run this to refresh the XML.
  /// </summary>
  public void RefreshMacroOrder() {
    ProgramXml.ReplaceMacroElements(Macros);
    // If we don't reload, relocating the macros jumbles them.
    // Perhaps there's a better way, but it broke when I tried.
    Save();
    Read();
    Log.WriteLine(
      $"{PathShort}: Saved and reloaded on reordering macros.");
  }

  public void RemoveDelayEffectsAndMacros() {
    BypassDelayEffects();
    if (RemoveDelayMacros()) {
      RefreshMacroOrder();
      InfoPageLayout.MoveMacrosToStandardLayout();
      ReUpdateMacroCcs();
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
    if (GuiScriptProcessor != null
        && !CanRemoveGuiScriptProcessor()) {
      Log.WriteLine(
        $"{PathShort}: Cannot remove macros because " +
        "there is a GUI script processor that is not feasible/desirable " +
        "to remove.");
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
  ///   Do away with the <see cref="GuiScriptProcessor" />, which defines a
  ///   special Info Page layout and MIDI CC numbers that modulate macros.
  ///   That will give the Info page the default appearance and allow macros to be moved,
  ///   added and removed. When the script processor has been dispensed with,
  ///   move the existing macros to locations optimal for accommodating a new wheel
  ///   replacement macro.
  /// </summary>
  private void RemoveGuiScriptProcessor() {
    GuiScriptProcessor!.Remove();
    GuiScriptProcessor = null;
    // GuiScriptProcessor!.Remove will have removed the EventProcessors
    // element. So we should clear ScriptProcessors for consistency. 
    ScriptProcessors = ScriptProcessors.Clear();
    // ReUpdateMacroCcs(); // ???
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
    NotifyUpdate($"{PathShort}: Removed Info Page CCs ScriptProcessor.");
    InfoPageLayout.MoveMacrosToStandardLayout();
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
    InfoPageLayout.ReplaceModWheelWithMacro();
    ReUpdateMacroCcs();
    NotifyUpdate($"{PathShort}: Replaced mod wheel with macro.");
  }

  private void ReUpdateMacroCcs() {
    UpdateMacroCcsOwnedByMacros();
    Log.WriteLine($"{PathShort}: Re-updated macro Ccs.");
  }

  public void RestoreOriginal() {
    string originalPath = System.IO.Path.Combine(
      Batch.GetOriginalProgramsFolderPath(),
      SoundBankName,
      Category.Name,
      System.IO.Path.GetFileName(Path));
    if (!Batch.FileSystemService.File.Exists(originalPath)) {
      throw new ApplicationException(
        $"Cannot find original file '{originalPath}' to restore to '{Path}'.");
    }
    CopyFile(originalPath, Path);
    Log.WriteLine($"{PathShort}: Restored to Original");
  }

  /// <summary>
  ///   Assuming any modulations by the wheel macro have already been reassigned to a
  ///   wheel macro, if there are at least 5 continuous macros, i.e. at least 1 more than
  ///   the usual number of expression pedals that can control them, make the best use
  ///   of the keyboard's hardware controllers by assigning MIDI CC 1 (mod wheel) to the
  ///   5th continuous macro and MIDI CC 11 (touch strip) to the 6th, if there is one.
  ///   Increment MIDI CCs of any subsequent macros accordingly.
  ///   <para>
  ///     For programs with a GUI script processor,
  ///     changing the MIDI CCs modulating macros is not (yet) supported by this method.
  ///     If it were to be implemented, 19 programs would be impacted, at last count by
  ///     <see cref="QueryReuseCc1NotSupported" />, all in Pulsar.
  ///   </para>
  /// </summary>
  public void ReuseCc1() {
    if (!Settings.MidiForMacros.HasModWheelReplacementCcNo) {
      return;
    }
    if (GuiScriptProcessor != null) {
      // See paragraph in summary.
      return;
    }
    if (ProgramXml.GetModulationElementsWithCcNo(1).Count > 0
        && !WheelMacroExists()) {
      // There are modulations whose MIDI CC number is 1, but they have not been
      // assigned to a wheel macro. If anything, that should be done instead of
      // assigning MIDI CC 1 to a different macro. 
      return;
    }
    if (SoundBankName == "Organic Pads") {
      // In InitialiseLayout, the Organic Pads wheel macro will have been placed at the
      // end, by design. So the normal algorithm for reusing MIDI CC 1 won't work.  
      var wheelMacro = FindWheelMacro()!;
      wheelMacro.ChangeCcNoTo(1);
      NotifyUpdate($"{PathShort}: Reused MIDI CC 1.");
      return;
    }
    var macroBeforeCc1Macro = (
      from macro in ContinuousMacros
      where macro.FindModulationWithCcNo(
        Settings.MidiForMacros.ModWheelReplacementCcNo) != null
      select macro).FirstOrDefault();
    if (macroBeforeCc1Macro == null) {
      return;
    }
    var continuousMacrosByLocation = (
      from continuousMacro in GetMacrosSortedByLocation(MacroCcLocationOrder)
      where continuousMacro.IsContinuous
      select continuousMacro).ToList();
    // There needs to be at least one macro after the macro before the macro whose
    // MIDI CC number is to be changed to 1!
    int minVisibleContinuousMacrosCount =
      continuousMacrosByLocation.IndexOf(macroBeforeCc1Macro) + 2;
    if (ContinuousMacros.Count < minVisibleContinuousMacrosCount) {
      // Allow for macros with invalid locations: see GetMacrosSortedByLocation. 
      return;
    }
    // Required for first call of GetNextCcNo to return 1.
    CurrentContinuousCcNo = Settings.MidiForMacros.ModWheelReplacementCcNo;
    CurrentToggleCcNo = 0; // Required by GetNextCcNo but won't be used in this case.
    for (int i = minVisibleContinuousMacrosCount - 1;
         i < continuousMacrosByLocation.Count;
         i++) {
      var macro = continuousMacrosByLocation[i];
      int newCcNo = GetNextCcNo(macro, true);
      macro.ChangeCcNoTo(newCcNo);
    }
    NotifyUpdate($"{PathShort}: Reused MIDI CC 1.");
  }

  public void Save() {
    ProgramXml.SaveToFile(Path);
  }

  private bool TryGetNonAdsrReleaseMacro(out Macro? releaseMacro) {
    var adsrMacros = GetAdsrMacros();
    if (adsrMacros.Count < 4
        && adsrMacros.TryGetValue("Release", out var macro)) {
      releaseMacro = macro;
    } else {
      releaseMacro = null;
    }
    return releaseMacro != null;
  }

  /// <summary>
  ///   Configures macro CCs.
  /// </summary>
  public void UpdateMacroCcs() {
    if (GuiScriptProcessor == null) {
      // The CCs are specified in Modulations owned by the Macros
      // (ConstantModulations) that they modulate
      UpdateMacroCcsOwnedByMacros();
    } else if (Category.TemplateScriptProcessor != null) {
      // The CCs are specified Modulations owned by the GUI ScriptProcessor
      // and can be copied from a template ScriptProcessor.
      // This applies to all programs in categories for which MustUseGuiScriptProcessor
      // is set to true in the settings file.
      GuiScriptProcessor.UpdateModulationsFromTemplate(
        Category.TemplateScriptProcessor.Modulations);
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
    // top to bottom, left to right. But a few, e.g. Factory\Keys\Days Of Old 1.4, do not.
    var sortedByLocation = GetMacrosSortedByLocation(MacroCcLocationOrder);
    // Reinitialise CurrentContinuousCcNo, incremented by GetNextCcNo, in case
    // UpdateMacroCcsInConstantModulations is called multiple times. It is called twice
    // by RemoveGuiScriptProcessor, the second time via
    // ReplaceModWheelWithMacro.
    // Make the first call of GetNextCcNo for a continuous macro return 31.
    CurrentContinuousCcNo = 0;
    // Make the first call of GetNextCcNo for a toggle macro return 112.
    CurrentToggleCcNo = 0;
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
      int ccNo = GetNextCcNo(macro, false);
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
        // In Factory\Keys\Days Of Old 1.4, Macro 1, a toggle macro, has Ratio -1 instead
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
  ///   Only used for Organic Pads sound bank in <see cref="FixCData" />.
  /// </summary>
  [ExcludeFromCodeCoverage]
  protected virtual void UpdateProgramFileWithFixedCData(string newContents) {
    using var writer = new StreamWriter(Path);
    writer.Write(newContents);
  }

  private bool WheelMacroExists() {
    return FindWheelMacro() != null;
  }

  /// <summary>
  ///   Sets the specified macro's value to zero if allowed.
  /// </summary>
  private void ZeroMacro(Macro macro) {
    if (macro.IsModulatedByWheel) {
      // Example: Titanium\Pads\Children's Choir.
      Log.WriteLine(
        $"{PathShort}: Not changing {macro.DisplayName} to zero because " +
        "it is modulated by the wheel.");
    } else {
      macro.ChangeValueToZero();
      NotifyUpdate($"{PathShort}: Changed {macro.DisplayName} to zero.");
    }
  }

  /// <summary>
  ///   If a Release macro is not part of a set of four ADSR macros, set its initial
  ///   value to zero.
  /// </summary>
  public void ZeroReleaseMacro() {
    if (TryGetNonAdsrReleaseMacro(out var releaseMacro)) {
      ZeroMacro(releaseMacro!);
      NotifyUpdate($"{PathShort}: Changed Release to zero.");
    }
  }

  public void ZeroReverbMacros() {
    if (GuiScriptProcessor is OrganicKeysScriptProcessor organicKeysScriptProcessor) {
      // "Organic Keys" sound bank
      organicKeysScriptProcessor.DelaySend = 0;
      organicKeysScriptProcessor.ReverbSend = 0;
      NotifyUpdate(
        $"{PathShort}: Changed '{organicKeysScriptProcessor.Name}'.DelaySend " +
        "and .ReverbSend to zero.");
    }
    var reverbMacros = (
      from macro in Macros
      where macro.ModulatesReverb
      select macro).ToList();
    if (reverbMacros.Count == 0) {
      return;
    }
    if (!Settings.CanChangeReverbToZero(SoundBankName, Category.Name, Name)) {
      // These programs are silent without reverb!
      Log.WriteLine($"Changing reverb to zero is disabled for '{PathShort}'.");
      return;
    }
    foreach (var reverbMacro in reverbMacros) {
      ZeroMacro(reverbMacro);
    }
  }
}