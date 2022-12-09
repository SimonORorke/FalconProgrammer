﻿using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

/// <summary>
///   In some sound banks, such as Organic Keys, ConstantModulations do not specify
///   Info page macros. In others, such as Hypnotic Drive, the macro locations
///   specified in ConstantModulations are not used on the Info page
///   (ConstantModulation.Properties showValue="0").  
///   Instead MIDI CC numbers in a ScriptProcessor need to be modelled
///   on a template program.
/// </summary>
public class ScriptConfig : ProgramConfig {
  private ScriptProcessor TemplateScriptProcessor { get; set; } = null!;
  [PublicAPI] public string TemplateScriptProcessorName { get; private set; } = null!;

  protected override ProgramXml CreateProgramXml() {
    return new ScriptProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor!);
  }

  public override void ConfigureMacroCcs(
    string soundBankName, string? categoryName = null) {
    TemplateSoundBankName = soundBankName;
    TemplateCategoryName = GetTemplateCategoryName();
    TemplateProgramName = GetTemplateProgramName();
    TemplateScriptProcessorName = GetTemplateScriptProcessorName();
    base.ConfigureMacroCcs(soundBankName, categoryName);
  }

  private void DeserialiseTemplateProgram() {
    using var reader = new StreamReader(TemplateProgramPath);
    var serializer = new XmlSerializer(typeof(UviRoot));
    var root = (UviRoot)serializer.Deserialize(reader)!;
    TemplateScriptProcessor =
      (from scriptProcessor in root.Program.ScriptProcessors
        where scriptProcessor.Name == TemplateScriptProcessorName
        select scriptProcessor).FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find {TemplateScriptProcessorName} in file '{TemplateProgramPath}'.");
  }

  protected override string GetInfoPageCcsScriptProcessorName() {
    if (SoundBankFolder.Name != "Voklm" || CategoryFolder.Name != "Vox Instruments") {
      return TemplateScriptProcessorName;
    }
    return "EventProcessor9"; // Voklm/Vox Instruments
  }

  private string GetTemplateCategoryName() {
    return TemplateSoundBankName switch {
      "Hypnotic Drive" => "Leads",
      "Organic Keys" => "Acoustic Mood",
      "Voklm" => "Synth Choirs",
      _ => throw new ApplicationException(
        $"A template category for sound bank '{TemplateSoundBankName}' " + 
        "has not yet been specified.")
    };
  }

  private string GetTemplateProgramName() {
    return TemplateSoundBankName switch {
      "Hypnotic Drive" => "Lead - Acid Gravel",
      "Organic Keys" => "A Rhapsody",
      "Voklm" => "Breath Five",
      _ => throw new ApplicationException(
        $"A template program name for sound bank '{TemplateSoundBankName}' " + 
        "has not yet been specified.")
    };
  }

  private string GetTemplateScriptProcessorName() {
    return TemplateSoundBankName switch {
      "Hypnotic Drive" => "EventProcessor99",
      "Organic Keys" => "EventProcessor0",
      "Voklm" => "EventProcessor0",
      _ => throw new ApplicationException(
        $"A template script processor name for sound bank '{TemplateSoundBankName}' " + 
        "has not yet been specified.")
    };
  }

  protected override void Initialise() {
    base.Initialise();
    DeserialiseTemplateProgram();
  }

  protected override void UpdateMacroCcs() {
    InfoPageCcsScriptProcessor!.SignalConnections.Clear();
    foreach (var signalConnection in TemplateScriptProcessor.SignalConnections) {
      InfoPageCcsScriptProcessor.SignalConnections.Add(signalConnection);
    }
    ProgramXml.UpdateInfoPageCcsScriptProcessor();
  }
}