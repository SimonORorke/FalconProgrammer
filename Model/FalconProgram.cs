using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer.Model;

internal class FalconProgram {
  private InfoPageLayout? _infoPageLayout;
  private SoundBankId? _soundBankId;

  public FalconProgram(string path, Category category, Batch batch) {
    Path = path;
    Name = System.IO.Path.GetFileNameWithoutExtension(path).Trim();
    Category = category;
    Batch = batch;
  }

  private Batch Batch { get; }
  protected Category Category { get; }

  /// <summary>
  ///   Gets continuous (as opposes to toggle) macros. It's safest to query this each
  ///   time, as it can change already when a delay macro is removed.
  /// </summary>
  internal List<Macro> ContinuousMacros => GetContinuousMacros();

  private ImmutableList<Effect> Effects { get; set; } = null!;
  internal ScriptProcessor? GuiScriptProcessor { get; private set; }
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
  public string Name { get; }

  [PublicAPI] public string Path { get; }

  [PublicAPI]
  public string PathShort =>
    $@"{SoundBankName}\{Category.Name}\{Name}";

  internal ProgramXml ProgramXml { get; private set; } = null!;

  public ImmutableList<ScriptProcessor> ScriptProcessors { get; private set; } =
    ImmutableList<ScriptProcessor>.Empty;

  public Settings Settings => Batch.Settings;

  /// <summary>
  ///   A sound bank identifier derived from program contents, so not dependent on
  ///   each sound bank folder name being the same as the publisher's sound bank name.
  /// </summary>
  public SoundBankId SoundBankId => _soundBankId ??= ProgramXml.GetSoundBankId();

  /// <summary>
  ///   The name of the sound bank folder. It does not necessarily need to be the same as
  ///   the publisher's sound bank name.
  /// </summary>
  private string SoundBankName => Category.SoundBankName;

  /// <summary>
  ///   Configures macro CCs.
  /// </summary>
  public void AssignMacroCcs() {
    if (Settings.MidiForMacros.ContinuousCcNos.Count == 0
        || Settings.MidiForMacros.ToggleCcNos.Count == 0) {
      throw new ApplicationException(
        "MIDI CC numbers cannot be assigned to macros " +
        "because ranges of continuous and/or toggle CC numbers have not yet " +
        "been specified. You need to do that on the MIDI for Macros page.");
    }
    if (GuiScriptProcessor == null) {
      if (Category.MustUseGuiScriptProcessor) {
        // If we don't throw this ApplicationException, a different exception will be
        // thrown. The category may have some programs with GUI script processors, others
        // without. Examples: Falcon Factory\Bass-Sub, Falcon Factory\Keys.
        // Such categories only exist in the Falcon Factory (version 1) sound bank.
        // There is a comment on this in FindGuiScriptProcessor.
        // Supporting use of the GUI script processors in those categories is unlikely to
        // be implemented, especially as all Falcon Factory rev2 programs have GUI script
        // processors.
        throw new ApplicationException(
          "Assigning MIDI CCs to macros for a program with a GUI script " +
          "processor is not supported for sound bank " +
          $"{SoundBankName} category {Category.Name}. You need to go to the " +
          "GUI Script Processor page and remove the sound bank or category from " +
          "the list. For the Falcon Factory (version 1) sound bank, the only " +
          "categories for which updating macro MIDI CCs for a GUI script processor " +
          " is supported are Brutal Bass 2.1', 'Lo-Fi 2.5', " +
          "'Organic Texture 2.8', 'RetroWave 2.5' and 'VCF-20 Synths 2.5'.");
      }
      // The CCs are specified in Modulations owned by the Macros
      // (ConstantModulations) that they modulate
      AssignMacroCcsOwnedByMacros();
    } else {
      var templateScriptProcessor = Category.GetTemplateScriptProcessor(
        GuiScriptProcessor, Batch);
      // The CCs are specified in Modulations owned by the GUI ScriptProcessor
      // and can be copied from a template ScriptProcessor.
      // This applies to all programs in categories for which MustUseGuiScriptProcessor
      // is set to true in the settings file.
      GuiScriptProcessor.AssignMacroCcsFromTemplate(
        templateScriptProcessor.Modulations, Macros);
    }
    NotifyUpdate($"{PathShort}: Assigned Macro CCs.");
  }

  /// <summary>
  ///   Where MIDI CC assignments to macros shown on the Info page are specified in
  ///   ConstantModulations,
  ///   updates the macro CCs so that the macros are successively assigned standard CCs
  ///   in the order of their locations on the Info page (top to bottom, left to right or
  ///   left to right, top to bottom, depending on <see cref="MacroCcLocationOrder" />).
  ///   There are different series of CCs for continuous and toggle macros.
  /// </summary>
  private void AssignMacroCcsOwnedByMacros() {
    // Most Falcon Factory programs list the ConstantModulation macro specifications in order
    // top to bottom, left to right. But a few, e.g. Falcon Factory\Keys\Days Of Old 1.4, do not.
    var sortedByLocation = GetMacrosSortedByLocation(MacroCcLocationOrder);
    // Make the first call of GetNextContinuousCcNo return the first continuous
    // CC number specified in settings.
    Settings.MidiForMacros.CurrentContinuousCcNo = 0;
    // Make the first call of GetNextToggleCcNo return the first
    // toggle CC number specified in settings.
    Settings.MidiForMacros.CurrentToggleCcNo = 0;
    foreach (var macro in sortedByLocation) {
      // Retain unaltered any mapping to the modulation wheel (MIDI CC 1) or any other
      // MIDI CC mapping that's not on the Info page.
      // Example: Devinity\Bass\Comber Bass.
      var forMacroModulations =
        macro.GetForMacroModulations();
      if (forMacroModulations.Count > 1) {
        throw new NotSupportedException(
          $"{PathShort}: Macro '{macro.DisplayNameWithoutCc}' " +
          $"owns {forMacroModulations.Count} 'for macro' Modulations.");
      }
      int ccNo = macro.IsContinuous
        ? Settings.MidiForMacros.GetNextContinuousCcNo(false)
        : Settings.MidiForMacros.GetNextToggleCcNo();
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
        // In Falcon Factory\Keys\Days Of Old 1.4, Macro 1, a toggle macro, has Ratio -1
        // instead of the usual 1. I don't know what the point of that is. But it
        // prevents the button controller mapped to the macro from working. To fix this,
        // if a toggle macro has Ratio -1, update Ratio to 1. I cannot see any
        // disadvantage in doing that. 
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (macro.IsToggle && modulation.Ratio == -1) {
          modulation.Ratio = 1;
        }
      }
      macro.AppendCcNoToDisplayName(ccNo);
    }
  }

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
    if (Category.MustUseGuiScriptProcessor) {
      return false;
    }
    if (GuiScriptProcessor?.GuiScriptId == ScriptId.OrganicTexture) {
      // Falcon Factory\Organic Texture 2.8
      throw new ApplicationException(
        "Removing the GUI script processor is not currently supported " +
        $"for programs of sound bank {SoundBankName} category {Category.Name}, " +
        "as multiple script parameters that can be controlled via the script-based " +
        "Info page GUI " +
        "are not represented by macros on the non-script-based Info page GUI. " +
        $"You need to add sound bank {SoundBankName} category {Category.Name} " +
        "to the list on the GUI Script Processor page.");
    }
    return SoundBankId switch {
      SoundBankId.OrganicKeys => throw new ApplicationException(
        "Removing the GUI script processor is not currently supported " +
        $"for programs of sound bank {SoundBankName}, as the Delay and Reverb " +
        "script parameters, which can be controlled via the script-based " +
        "Info page GUI, " +
        "are not represented by macros on the non-script-based Info page GUI. " +
        $"You need to add sound bank {SoundBankName} to the list on the " +
        "GUI Script Processor page."),
      SoundBankId.Pulsar => throw new ApplicationException(
        "Removing the GUI script processor is not currently supported " +
        $"for programs of sound bank {SoundBankName}, as there are too many " +
        "macros for the non-script-based Info page GUI. " +
        $"You need to add sound bank {SoundBankName} to the list on the " +
        "GUI Script Processor page."),
      SoundBankId.Voklm => throw new ApplicationException(
        "Removing the GUI script processor is not currently supported " +
        $"for programs of sound bank {SoundBankName}, as MIDI modulations cannot be " +
        "reliably assigned to macros on the non-script-based Info page GUI. " +
        $"You need to add sound bank {SoundBankName} to the list on the " +
        "GUI Script Processor page."),
      _ => true
    };
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
    if (SoundBankId is SoundBankId.EtherFields) {
      Log.WriteLine(
        $"{PathShort}: Replacing the mod wheel with a macro is not supported " +
        $"for sound bank {SoundBankName}, " +
        $"due to macro count and complexity.");
      return false;
    }
    if (GuiScriptProcessor != null
        && !CanRemoveGuiScriptProcessor()) {
      Log.WriteLine(
        $"{PathShort}: Replacing the mod wheel with a macro is not supported " +
        $"because the program's Info page GUI is specified in a script processor.");
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
      // Example: Falcon Factory\Pads\DX FM Pad 2.0
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

  private Macro? FindReverbContinuousMacro() {
    return (
      from macro in ContinuousMacros
      where macro.ModulatesReverb
      select macro).FirstOrDefault();
  }

  private Macro? FindReverbToggleMacro() {
    return (
      from macro in Macros
      where macro.IsToggle && macro.ModulatesReverb
      select macro).FirstOrDefault();
  }

  [SuppressMessage("ReSharper", "CommentTypo")]
  private Macro? FindWheelMacro() {
    return (
      from continuousMacro in ContinuousMacros
      // I changed this to look for DisplayName is 'Wheel' instead of DisplayName
      // contains 'Wheel'. The only two additional programs that subsequently got wheel
      // macros added were
      // Falcon Factory\Hybrid Perfs\Louis Funky Dub and Falcon Factory\Pluck\Permuda 1.1.
      //
      // In Louis Funky Dub, the original 'Wheel me' macro did and does nothing I can
      // hear. The mod wheel did work, and the added wheel macro has successfully
      // replaced it. So the problem with the 'Wheel me' macro, if it's real, has not
      // been caused by the wheel replacement macro implementation.
      //
      // In Permuda 1.1, the original 'Modwheel' macro controlled the range of the mod
      // wheel and now controls the range of the 'Wheel' macro instead.  So that change
      // has worked too.
      where continuousMacro.DisplayNameWithoutCc.Equals(
        "wheel", StringComparison.CurrentCultureIgnoreCase)
      select continuousMacro).FirstOrDefault();
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

  internal List<Macro> GetMacrosSortedByLocation(
    LocationOrder macroCcLocationOrder) {
    var sortedSet = new SortedSet<Macro>(
      macroCcLocationOrder == LocationOrder.TopToBottomLeftToRight
        ? new TopToBottomLeftToRightComparer()
        : new LeftToRightTopToBottomComparer());
    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
    foreach (var macro in Macros) {
      // This validation is not reliable. In "Falcon Factory\Bells\Glowing 1.2", the macros with
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
        sortedSet.Add(macro);
      }
    }
    return sortedSet.ToList();
  }

  private bool HasUniqueLocation(Macro macro) {
    return (
      from m in Macros
      where m.X == macro.X
            && m.Y == macro.Y
      select m).Count() == 1;
  }

  private void InitialiseDahdsrControllerScripts() {
    string superFolderPath = System.IO.Path.GetDirectoryName(
      Settings.ProgramsFolder.Path)!;
    string scriptsFolderPath = System.IO.Path.Combine(superFolderPath, "Scripts");
    string stubScriptPath = System.IO.Path.Combine(
      scriptsFolderPath, "DAHDSR Controller.lua");
    string scriptSubFolderPath = System.IO.Path.Combine(
      scriptsFolderPath, "DahdsrController");
    string requireScriptPath = System.IO.Path.Combine(
      scriptSubFolderPath, "DahdsrController.lua");
    if (!Batch.FileSystemService.File.Exists(stubScriptPath)) {
      throw new ApplicationException(GetErrorMessage(stubScriptPath));
    }
    if (!Batch.FileSystemService.File.Exists(requireScriptPath)) {
      throw new ApplicationException(GetErrorMessage(requireScriptPath));
    }
    // Script parameter substitution.
    const string attackPlaceholder = "MAX_ATTACK_SECONDS";
    const string decayPlaceholder = "MAX_DECAY_SECONDS";
    const string releasePlaceholder = "MAX_RELEASE_SECONDS";
    string maxAttackSeconds =
      Settings.SoundBankSpecific.OrganicPads.MaxAttackSeconds.ToString();
    string maxDecaySeconds =
      Settings.SoundBankSpecific.OrganicPads.MaxDecaySeconds.ToString();
    string maxReleaseSeconds =
      Settings.SoundBankSpecific.OrganicPads.MaxReleaseSeconds.ToString();
    using var reader = new StreamReader(
      Global.GetEmbeddedFileStream("DahdsrController.lua"));
    string template = reader.ReadToEnd();
    string script = template.Replace(attackPlaceholder, maxAttackSeconds)
      .Replace(decayPlaceholder, maxDecaySeconds)
      .Replace(releasePlaceholder, maxReleaseSeconds);
    WriteTextToFile(requireScriptPath, script);
    return;

    string GetErrorMessage(string scriptPath) {
      string applicationFolderPath = AppDomain.CurrentDomain.BaseDirectory;
      string applicationScriptsSubfolderPath =
        System.IO.Path.Combine(applicationFolderPath, "Scripts");
      var writer = new StringWriter();
      writer.WriteLine($"Cannot find script file '{scriptPath}'.");
      writer.WriteLine(
        $"Removing the {SoundBankName} sound bank's GUI script processor requires " +
        "a replacement non-GUI script processor that needs two script files: " +
        $"'{stubScriptPath}' and '{requireScriptPath}'.");
      writer.WriteLine("You have two options:");
      writer.WriteLine();
      writer.WriteLine(
        $"1) Copy the script files from the {Global.ApplicationName} application " +
        $"folder's Scripts subfolder '{applicationScriptsSubfolderPath}'.");
      writer.WriteLine();
      writer.Write(
        $"2) Disable GUI script processor removal for the {SoundBankName} sound bank " +
        "by adding the sound bank to the list on the the GUI Script Processor page.");
      return writer.ToString();
    }
  }

  /// <summary>
  ///   Initialises the program, with options specified on the Initialisation and
  ///   Background pages.
  ///   <para>
  ///     First, unless the sound bank or category is on the GUI Script Processor list
  ///     on the Initialisation page, removes the GUI script processor, if there is one,
  ///     so that the default Info Page layout will be shown. The macros on the
  ///     default Info Page will be arranged into a standard layout
  ///   </para>
  ///   <para>
  ///     The remaining procedures are executed only if the default Info page layout
  ///     is shown, with the GUI script processor, if there was one, having just
  ///     been removed.
  ///     <list type="bullet">
  ///       <item>
  ///         <description>
  ///           Sets the background image for the Info page, if one had been specified
  ///           for the sound bank on the Background page.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           If specified by options on the Initialisation page,
  ///           for sound banks Ether Fields and Spectre, rearranges the macros into a
  ///           standard layout.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           If specified by an option on the Initialisation page,
  ///           for sound bank Fluidity, moves the Attack macro, if any, to be the last
  ///           macro in the Info page layout.
  ///         </description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           For sound bank Organic Pads:
  ///           <para>
  ///             Adds a macro for each Layer gain,
  ///             to make variation of these parameters mutually independent,
  ///             and as the X-Y control on the script-based GUI cannot be implemented
  ///             on the default Info page layout.
  ///           </para>
  ///           <para>
  ///             Bypasses all delay and reverb effects.
  ///             The Organic Pads GUI script processor has delay and reverb
  ///             parameters, controllable from the script-based GUI.
  ///             There is no way to replicate control of these delay and reverb
  ///             modulations with macros on a default Info page layout.
  ///           </para>
  ///           <para>
  ///             Initialises maximum attack seconds, maximum delay seconds and
  ///             maximum release seconds to values specified the Initialisation page.
  ///           </para>
  ///           <para>
  ///             Optionally initialises attack seconds and release seconds to values
  ///             specified the Initialisation page.
  ///           </para>
  ///         </description>
  ///       </item>
  ///     </list>
  ///   </para>
  /// </summary>
  /// <remarks>
  /// </remarks>
  public void InitialiseLayout() {
    // There can be a delay loading programs with GUI script processors, for example
    // nearly 10 seconds for Modular Noise\Keys\Inscriptions. So remove the GUI script
    // processor, if there is one, unless there is a need to keep it.
    // This saves having to decide later whether to remove the GUI script processor,
    // such as to add a wheel macro or remove a delay macro.
    if (GuiScriptProcessor != null && CanRemoveGuiScriptProcessor()) {
      RemoveGuiScriptProcessor();
    }
    if (GuiScriptProcessor != null) {
      return;
    }
    if (Settings.TryGetSoundBankBackgroundImagePath(
          SoundBankName, out string path)) {
      SetBackgroundImagePath();
    }
    switch (SoundBankId) {
      case SoundBankId.EtherFields:
        if (Settings.SoundBankSpecific.EtherFields.StandardLayout) {
          InfoPageLayout.MoveMacrosToStandardLayout();
        }
        break;
      case SoundBankId.Fluidity:
        if (Settings.SoundBankSpecific.Fluidity.MoveAttackMacroToEnd) {
          var attackMacro = FindContinuousMacro("Attack");
          if (attackMacro != null) {
            MoveMacroToEnd(attackMacro);
            RefreshMacroOrder();
          }
        }
        break;
      case SoundBankId.OrganicPads:
        InitialiseOrganicPadsProgram();
        break;
      case SoundBankId.Spectre:
        if (Settings.SoundBankSpecific.Spectre.StandardLayout) {
          InfoPageLayout.MoveMacrosToStandardLayout();
        }
        break;
    }
    return;

    void SetBackgroundImagePath() {
      string relativePath =
        System.IO.Path.GetRelativePath(Category.Path, path);
      string falconFormatPath = "./" + relativePath.Replace(@"\", "/");
      ProgramXml.BackgroundImagePath = falconFormatPath;
      NotifyUpdate($"{PathShort}: Set BackgroundImagePath.");
    }
  }

  private void InitialiseOrganicPadsProgram() {
    InitialiseDahdsrControllerScripts();
    // Script files exist. Good to go.
    ProgramXml.CopyMacroElementsFromTemplate("OrganicPads_Macros.xml");
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
    var layers = ProgramXml.GetLayers(Settings.MidiForMacros);
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
    ProgramXml.AddScriptProcessorElementFromTemplate("OrganicPads_DahdsrController.xml");
    NotifyUpdate($"{PathShort}: Added ScriptProcessor.");
    // Initialise the DAHDSR attack and release times to subvert the original intention 
    // for the sound to be a pad!
    // A problem still to solve is that reducing the attack time to zero while playing
    // can cause low volume and then silence. See the comment at the top of the
    // DahdsrController script for details.
    // So, for safety, that's another reason to initialise the attack time to be fast.
    var mainDahdsr = ProgramXml.FindMainDahdsr(Settings.MidiForMacros);
    if (mainDahdsr == null) {
      throw new InvalidOperationException(
        $"{PathShort}: Cannot find DAHDSR in ControlSignalSources.");
    }
    if (Settings.SoundBankSpecific.OrganicPads.AttackSeconds >= 0) {
      mainDahdsr.AttackTime = Settings.SoundBankSpecific.OrganicPads.AttackSeconds;
      NotifyUpdate(
        $"{PathShort}: Initialised '{mainDahdsr.DisplayName}'.AttackTime.");
    }
    if (Settings.SoundBankSpecific.OrganicPads.ReleaseSeconds >= 0) {
      mainDahdsr.ReleaseTime = Settings.SoundBankSpecific.OrganicPads.ReleaseSeconds;
      NotifyUpdate(
        $"{PathShort}: Initialised '{mainDahdsr.DisplayName}'.ReleaseTime.");
    }
    // The GUI script processor has delaySend and reverbSend parameters, controllable 
    // from the script-based GUI. With the new script processor, there's no way to
    // replicate the delay and reverb modulations implemented int the GUI script.
    // So we need to bypass all delay and reverb effects.
    BypassDelayEffects();
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
  /// <remarks>
  ///   <list type="bullet">
  ///     <listheader>
  ///       <description>Prerequisites</description>
  ///     </listheader>
  ///     <item>
  ///       <description>
  ///         The sound bank\category is not included in setting
  ///         <see cref="Settings.MustUseGuiScriptProcessorCategories" />.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         The GUI script processor, if any, has been removed by
  ///         <see cref="InitialiseLayout" />.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         <see cref="ZeroReleaseMacro" /> and/or <see cref="ZeroReverbMacros" />
  ///         have been run.
  ///       </description>
  ///     </item>
  ///   </list>
  /// </remarks>
  public void MoveZeroedMacrosToEnd() {
    if (GuiScriptProcessor != null) {
      Log.WriteLine(
        $"{PathShort}: Cannot move macros because " +
        "because the program's Info page GUI is specified in a script processor.");
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
      UpdateMacroCcs();
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
      NotifyUpdate(
        $"{PathShort}: Moved {macro.DisplayNameWithoutCc} macro to end.");
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
        connectionsParent = new Effect(connectionsParentElement, ProgramXml,
          Settings.MidiForMacros);
        effects.Add((Effect)connectionsParent);
        // Indicate that the Effect has now been added to Effects.
        effectElements.Remove(connectionsParentElement);
      } else {
        connectionsParent = new ConnectionsParent(connectionsParentElement, ProgramXml,
          Settings.MidiForMacros);
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
      select new Effect(effectElement, ProgramXml, Settings.MidiForMacros));
    Effects = effects.ToImmutableList();
  }

  /// <summary>
  ///   Prepends a line indicating the program's short path to the program's description,
  ///   which is viewable in Falcon when the Info page's  "i" button is clicked.
  /// </summary>
  /// <remarks>
  ///   This task must be done last! Due to the fix required to conserve paragraph breaks
  ///   in the description, updates by subsequent tasks causes further description
  ///   changes and somehow messes up wheel macro changes.
  /// </remarks>
  public void PrependPathLineToDescription() {
    const string pathIndicator = "PATH: ";
    string blankLine = Environment.NewLine + Environment.NewLine;
    ProgramXml.InitialiseDescription();
    string oldDescription = ProgramXml.GetDescription();
    string oldPathLine =
      oldDescription.StartsWith(pathIndicator) && oldDescription.Contains(blankLine)
        ? oldDescription[..(oldDescription.IndexOf(blankLine) + 4)]
        : string.Empty;
    string oldRestOfDescription = oldPathLine == string.Empty
      ? oldDescription
      : oldDescription.Replace(oldPathLine, string.Empty);
    string newPathLine = pathIndicator + PathShort + blankLine;
    // When the program file is read, the paragraph breaks (i.e. blank lines) are
    // unhelpfully replaced with pairs of spaces.
    // See the comment in ProgramXml.ReadRootElementFromXmlText".
    // So reverse that to conserve the paragraph breaks.
    string newRestOfDescription = oldRestOfDescription.Replace(
      "  ", blankLine);
    string newDescription = newPathLine + newRestOfDescription;
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
    var dahdsrs = ProgramXml.GetDahdsrs(Settings.MidiForMacros);
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
    var mainDahdsr = ProgramXml.FindMainDahdsr(Settings.MidiForMacros);
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
        SoundBankId, scriptProcessorElement, ProgramXml,
        Settings.MidiForMacros, Category.MustUseGuiScriptProcessor)).ToImmutableList();
    foreach (var scriptProcessor in ScriptProcessors) {
      foreach (var modulation in scriptProcessor.Modulations) {
        // Needed for modulation.ModulatesMacro in FindGuiScriptProcessor 
        modulation.Owner = scriptProcessor;
      }
    }
    GuiScriptProcessor = Category.FindGuiScriptProcessor(ScriptProcessors);
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

  /// <summary>
  ///   Bypasses (disables) all known delay effects and then, if the program does not
  ///   have a GUI script processor, removes any macro that no longer modulates any
  ///   enabled effects.
  /// </summary>
  public void RemoveDelayEffectsAndMacros() {
    BypassDelayEffects();
    if (RemoveDelayMacros()) {
      RefreshMacroOrder();
      InfoPageLayout.MoveMacrosToStandardLayout();
      UpdateMacroCcs();
    }
    if (GuiScriptProcessor is OrganicGuiScriptProcessor organicScriptProcessor) {
      // "Organic Keys" or "Organic Pads" sound bank
      organicScriptProcessor.DelaySend = 0;
      NotifyUpdate(
        $"{PathShort}: Changed '{organicScriptProcessor.Name}'.DelaySend to zero.");
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
        "because the program's Info page GUI is specified in a script processor.");
      return false;
    }
    foreach (var macro in removableMacros) {
      macro.RemoveElement();
      Macros.Remove(macro);
      NotifyUpdate($"{PathShort}: Removed {macro.DisplayNameWithoutCc}.");
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
  ///   parameters with modulations by a new 'Wheel' macro.
  /// </summary>
  /// <remarks>
  ///   <list type="bullet">
  ///     <listheader>
  ///       <description>Prerequisites</description>
  ///     </listheader>
  ///     <item>
  ///       <description>
  ///         The sound bank\category is not included in setting
  ///         <see cref="Settings.MustUseGuiScriptProcessorCategories" />.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         The GUI script processor, if any, has been removed by
  ///         <see cref="InitialiseLayout" />.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         There is not already a macro with display name 'Wheel'.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///       </description>
  ///     </item>
  ///   </list>
  ///   <list type="bullet">
  ///     <listheader>
  ///       <description>Exclusions</description>
  ///     </listheader>
  ///     <item>
  ///       <description>
  ///         Not supported for sound banks Ether Fields or Organic Keys.
  ///         The programs in these sound banks are too complex for this
  ///         configuration task to be feasible, for now at least.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         The program already has a macro with display name 'Wheel'.
  ///       </description>
  ///     </item>
  ///   </list>
  /// </remarks>
  public void ReplaceModWheelWithMacro() {
    if (!CanReplaceModWheelWithMacro()) {
      return;
    }
    InfoPageLayout.ReplaceModWheelWithMacro();
    UpdateMacroCcs();
    NotifyUpdate($"{PathShort}: Replaced mod wheel with macro.");
  }

  /// <summary>
  ///   Restores the program file in a sound bank\category subfolder of the
  ///   Program Files folder from the corresponding file in the Original Program Files
  ///   folder. 
  /// </summary>
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
    Log.WriteLine($"{PathShort}: Restored to original.");
  }

  /// <summary>
  ///   Assuming any modulations by the wheel macro have already been reassigned to a
  ///   wheel macro, if there are at least 5 continuous macros, i.e. at least 1 more than
  ///   the usual number of expression pedals that can control them, make the best use
  ///   of the keyboard's hardware controllers by assigning MIDI CC 1 (mod wheel) to the
  ///   5th continuous macro and MIDI CC 11 (touch strip) to the 6th, if there is one.
  ///   Increment MIDI CCs of any subsequent macros accordingly.
  /// </summary>
  /// <remarks>
  ///   <list type="bullet">
  ///     <listheader>
  ///       <description>Prerequisites</description>
  ///     </listheader>
  ///     <item>
  ///       <description>
  ///         Setting <see cref="MidiForMacros.ModWheelReplacementCcNo" /> > 1.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         The sound bank\category is not included in setting
  ///         <see cref="Settings.MustUseGuiScriptProcessorCategories" />.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         The GUI script processor, if any, has been removed by
  ///         <see cref="InitialiseLayout" />.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         There is a macro modulation with source
  ///         <see cref="MidiForMacros.ModWheelReplacementCcNo" /> and at least one
  ///         more after it.
  ///       </description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         There are no macro modulations whose MIDI CC number is 1 but have not been
  ///         assigned to a wheel macro by <see cref="RemoveDelayEffectsAndMacros" />.
  ///         If anything, that should be done instead of assigning MIDI CC 1 to a
  ///         different macro.
  ///       </description>
  ///     </item>
  ///   </list>
  ///   It would be possible but not desirable for ReuseCc1 to support programs with
  ///   GUI script processors. For example, only 19 Pulsar programs (that I use anyway)
  ///   don't use the mod wheel and so could reuse CC 1. Pulsar programs have 30 macros,
  ///   all continuous. Using CC 1 for the fifth macro and incrementing the CC numbers
  ///   of the following numbers for some Pulsar programs but not others would lead to
  ///   two inconsistent CC numbering schemas. An it would not be easy to tell them
  ///   apart, as it is not possible to append the CC number to the displayed name of
  ///   a macro when the Info page's GUI is provided by a script.
  /// </remarks>
  public void ReuseCc1() {
    if (!Settings.MidiForMacros.HasModWheelReplacementCcNo) {
      return;
    }
    if (GuiScriptProcessor != null) {
      return;
    }
    if (ProgramXml.GetModulationElementsWithCcNo(1).Count > 0
        && !WheelMacroExists()) {
      return;
    }
    if (SoundBankId == SoundBankId.OrganicPads) {
      // In InitialiseLayout, the Organic Pads wheel macro will have been placed at the
      // end, by design. So the normal algorithm for reusing MIDI CC 1 won't work.  
      var wheelMacro = FindWheelMacro();
      if (wheelMacro != null) {
        wheelMacro.ChangeCcNoTo(1);
        NotifyUpdate($"{PathShort}: Reused MIDI CC 1.");
      }
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
    // Required for first call of GetNextContinuousCcNo to return 1.
    Settings.MidiForMacros.CurrentContinuousCcNo =
      Settings.MidiForMacros.ModWheelReplacementCcNo;
    for (int i = minVisibleContinuousMacrosCount - 1;
         i < continuousMacrosByLocation.Count;
         i++) {
      var macro = continuousMacrosByLocation[i];
      int newCcNo = Settings.MidiForMacros.GetNextContinuousCcNo(true);
      macro.ChangeCcNoTo(newCcNo);
      macro.AppendCcNoToDisplayName(newCcNo);
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

  private void UpdateMacroCcs() {
    AssignMacroCcsOwnedByMacros();
    Log.WriteLine($"{PathShort}: Updated macro Ccs.");
  }

  private bool WheelMacroExists() {
    return FindWheelMacro() != null;
  }

  [ExcludeFromCodeCoverage]
  protected virtual void WriteTextToFile(string path, string contents) {
    File.WriteAllText(path, contents);
  }

  /// <summary>
  ///   Sets the specified macro's value to zero if allowed.
  /// </summary>
  private void ZeroMacro(Macro macro) {
    if (macro.IsModulatedByWheel) {
      // Example: Titanium\Pads\Children's Choir.
      Log.WriteLine(
        $"{PathShort}: Not changing {macro.DisplayNameWithoutCc} to zero because " +
        "it is modulated by the wheel.");
    } else {
      macro.ChangeValueToZero();
      NotifyUpdate($"{PathShort}: Changed {macro.DisplayNameWithoutCc} to zero.");
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
    if (GuiScriptProcessor is OrganicGuiScriptProcessor organicScriptProcessor) {
      // "Organic Keys" or "Organic Pads" sound bank
      organicScriptProcessor.ReverbSend = 0;
      NotifyUpdate(
        $"{PathShort}: Changed '{organicScriptProcessor.Name}'.ReverbSend to zero.");
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