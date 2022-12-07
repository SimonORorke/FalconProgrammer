using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml {
  private XElement? _templateSignalConnectionElement;

  public ProgramXml(
    string templateProgramPath, ScriptProcessor? infoPageCcsScriptProcessor = null) {
    TemplateProgramPath = templateProgramPath;
    InfoPageCcsScriptProcessor = infoPageCcsScriptProcessor;
  }

  private List<XElement> ConstantModulationElements { get; set; } = null!;
  [PublicAPI] public string InputProgramPath { get; set; } = null!;
  private ScriptProcessor? InfoPageCcsScriptProcessor { get; }
  protected XElement? InfoPageCcsScriptProcessorElement { get; private set; }
  private XElement RootElement { get; set; } = null!;
  [PublicAPI] public string TemplateProgramPath { get; }

  private XElement TemplateSignalConnectionElement =>
    _templateSignalConnectionElement ??= GetTemplateSignalConnectionElement();

  public void AddConstantModulationSignalConnection(
    SignalConnection signalConnection, int index) {
    var constantModulationElement = ConstantModulationElements[index];
    // If there's already a modulation wheel assignment, the ConstantModulation element
    // will already own a Connections element. 
    var connectionsElement = constantModulationElement.Element("Connections");
    if (connectionsElement == null) {
      connectionsElement = new XElement("Connections"); 
      constantModulationElement.Add(connectionsElement);
    }
    connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
  }

  private XElement CreateSignalConnectionElement(SignalConnection signalConnection) {
    var result = new XElement(TemplateSignalConnectionElement);
    // In case the template SignalConnection contains a non-default (< 1) Ratio,
    // set the Ratio to the default, 1.
    var ratioAttribute =
      result.Attribute("Ratio") ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.{nameof(SignalConnection.Ratio)} "
        + $"attribute in '{TemplateProgramPath}'.");
    ratioAttribute.Value = "1";
    UpdateSignalConnectionElement(signalConnection, result);
    return result;
  }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = XElement.Load(InputProgramPath);
      var controlSignalSourcesElement =
        RootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
        throw new ApplicationException(
          $"Cannot find ControlSignalSources element in '{TemplateProgramPath}'.");
      ConstantModulationElements = controlSignalSourcesElement.Elements(
        "ConstantModulation").ToList();
      InfoPageCcsScriptProcessorElement = null;
      if (InfoPageCcsScriptProcessor != null) {
        var eventProcessorsElement =
          RootElement.Descendants("EventProcessors").FirstOrDefault();
        if (eventProcessorsElement != null) {
          var scriptProcessorElements = eventProcessorsElement.Elements(
            "ScriptProcessor");
          InfoPageCcsScriptProcessorElement = (
            from scriptProcessorElement in scriptProcessorElements
            where scriptProcessorElement.Attribute("Name")!.Value ==
                  InfoPageCcsScriptProcessor!.Name
            select scriptProcessorElement).FirstOrDefault();
        }
      }
    } catch (XmlException ex) {
      throw new ApplicationException(
        $"The following XML error was found in '{InputProgramPath}'\r\n:{ex.Message}");
    }
  }

  protected virtual XElement GetTemplateSignalConnectionElement() {
    var rootElement = XElement.Load(TemplateProgramPath);
    var result =
      rootElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find SignalConnection element in '{TemplateProgramPath}'.");
    return result;
  }

  public void SaveToFile(string outputProgramPath) {
    try {
      var writer = XmlWriter.Create(
        outputProgramPath,
        new XmlWriterSettings {
          Indent = true,
          IndentChars = "    "
        });
      RootElement.WriteTo(writer);
      writer.Close();
    } catch (XmlException ex) {
      throw new ApplicationException(
        $"The following XML error was found on writing to '{outputProgramPath}'\r\n:{ex.Message}");
    }
  }

  public void UpdateConstantModulationSignalConnection(
    ConstantModulation constantModulation, 
    SignalConnection signalConnection) {
    var constantModulationElement = ConstantModulationElements[constantModulation.Index];
    var connectionsElement = constantModulationElement.Element("Connections")!;
    var signalConnectionElements = 
      connectionsElement.Elements("SignalConnection").ToList();
    // The ConstantModulation will have two SignalConnections if one of them maps to the
    // modulation wheel (MIDI CC 1). 
    var signalConnectionElement = signalConnectionElements[signalConnection.Index]; 
    UpdateSignalConnectionElement(signalConnection, signalConnectionElement);
  }

  public virtual void UpdateInfoPageCcsScriptProcessor() {
    var connectionsElement = InfoPageCcsScriptProcessorElement!.Element("Connections");
    if (connectionsElement != null) {
      connectionsElement.RemoveAll();
    } else {
      connectionsElement = new XElement("Connections");
      InfoPageCcsScriptProcessorElement.Add(connectionsElement);
    }
    foreach (var signalConnection in InfoPageCcsScriptProcessor!.SignalConnections) {
      connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
    }
  }

  private void UpdateSignalConnectionElement(
    SignalConnection signalConnection, XElement signalConnectionElement) {
    var ratioAttribute =
      signalConnectionElement.Attribute("Ratio") ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.{nameof(SignalConnection.Ratio)} "
        + $"attribute in '{InputProgramPath}'.");
    ratioAttribute.Value = signalConnection.Ratio.ToString();
    var sourceAttribute =
      signalConnectionElement.Attribute(nameof(SignalConnection.Source)) ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.{nameof(SignalConnection.Source)} "
        + $"attribute in '{InputProgramPath}'.");
    sourceAttribute.Value = signalConnection.Source;
    var destinationAttribute =
      signalConnectionElement.Attribute(nameof(SignalConnection.Destination)) ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.{nameof(SignalConnection.Destination)} "
        + $"attribute in '{InputProgramPath}'.");
    destinationAttribute.Value = signalConnection.Destination;
  }
}