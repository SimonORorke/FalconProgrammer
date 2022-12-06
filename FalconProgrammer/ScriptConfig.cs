using System.Xml.Serialization;
using FalconProgrammer.XmlDeserialised;
using FalconProgrammer.XmlLinq;
using JetBrains.Annotations;

namespace FalconProgrammer;

public class ScriptConfig : ProgramConfig {
  public ScriptConfig(string templateSoundBankName, string templateCategoryName,
    string templateProgramName,
    string templateScriptProcessorName) : base(
    templateSoundBankName, templateCategoryName, templateProgramName) {
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