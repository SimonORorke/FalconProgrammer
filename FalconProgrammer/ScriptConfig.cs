using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

/// <summary>
///   In some sound banks, such as Organic Keys, ConstantModulations do not specify
///   Info page macros. In others, such as Hypnotic Drive, the macro locations
///   specified in ConstantModulations are not used on the Info page.  
///   Instead MIDI CC numbers in a ScriptProcessor need to be modelled
///   on a template program.
/// </summary>
public class ScriptConfig : ProgramConfig {
  public ScriptConfig(string templateProgramPath,
    string templateScriptProcessorName) : base(templateProgramPath) {
    InfoPageCcsScriptProcessorName = templateScriptProcessorName;
    TemplateScriptProcessorName = templateScriptProcessorName;
  }

  private ScriptProcessor TemplateScriptProcessor { get; set; } = null!;
  [PublicAPI] public string TemplateScriptProcessorName { get; }

  protected override ProgramXml CreateProgramXml() {
    return new ScriptProgramXml(TemplateProgramPath, InfoPageCcsScriptProcessor!);
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