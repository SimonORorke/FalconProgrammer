using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml {
  private XElement? _templateSignalConnectionElement;

  public ProgramXml(string templatePath, ProgramConfig programConfig) {
    TemplatePath = templatePath;
    ProgramConfig = programConfig;
  }

  private List<XElement> ConstantModulationElements { get; set; } = null!;
  [PublicAPI] public string InputPath { get; set; } = null!;
  protected XElement? MacroCcsScriptProcessorElement { get; private set; }
  private ProgramConfig ProgramConfig { get; }
  private XElement RootElement { get; set; } = null!;
  [PublicAPI] public string TemplatePath { get; }

  private XElement TemplateSignalConnectionElement =>
    _templateSignalConnectionElement ??= GetTemplateSignalConnectionElement();

  public void AddConstantModulationSignalConnection(
    SignalConnection signalConnection, int index) {
    var constantModulationElement = ConstantModulationElements[index];
    var connectionsElement = new XElement("Connections");
    constantModulationElement.Add(connectionsElement);
    connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
  }
  
  private XElement CreateSignalConnectionElement(SignalConnection signalConnection) {
    var result = new XElement(TemplateSignalConnectionElement);
    // In case the template SignalConnection contains a non-default (< 1) Ratio,
    // set the Ratio to the default, 1.
    var ratioAttribute = 
      result.Attribute("Ratio") ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.Ratio attribute in '{TemplatePath}'.");
    ratioAttribute.Value = "1";
    UpdateSignalConnectionElement(signalConnection, result);
    return result;
  }

  public void LoadFromFile(string inputPath) {
    InputPath = inputPath;
    try {
      RootElement = XElement.Load(InputPath);
      var controlSignalSourcesElement =
        RootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
        throw new ApplicationException(
          $"Cannot find ControlSignalSources element in '{TemplatePath}'.");
      ConstantModulationElements = controlSignalSourcesElement.Elements(
        "ConstantModulation").ToList();
      var eventProcessorsElement =
        RootElement.Descendants("EventProcessors").FirstOrDefault();
      if (eventProcessorsElement != null) {
        var scriptProcessorElements = eventProcessorsElement.Elements(
          "ScriptProcessor");
        MacroCcsScriptProcessorElement = (
          from scriptProcessorElement in scriptProcessorElements
          where scriptProcessorElement.Attribute("Name")!.Value ==
                ProgramConfig.MacroCcsScriptProcessorName
          select scriptProcessorElement).FirstOrDefault();
      }
    } catch (XmlException ex) {
      throw new ApplicationException(
        $"The following XML error was found in '{InputPath}'\r\n:{ex.Message}");
    }
  }

  private XElement GetTemplateSignalConnectionElement() {
    var root = XElement.Load(TemplatePath);
    var result =
      root.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find SignalConnection element in '{TemplatePath}'.");
    return result;
  }

  public void SaveToFile(string outputPath) {
    try {
      var writer = XmlWriter.Create(
        outputPath,
        new XmlWriterSettings {
          Indent = true,
          IndentChars = "    "
        });
      RootElement.WriteTo(writer);
      writer.Close();
    } catch (XmlException ex) {
      throw new ApplicationException(
        $"The following XML error was found on writing to '{outputPath}'\r\n:{ex.Message}");
    }
  }

  public void UpdateConstantModulationSignalConnection(
    SignalConnection signalConnection, int index) {
    var constantModulationElement = ConstantModulationElements[index];
    var connectionsElement = constantModulationElement.Element("Connections")!;
    var signalConnectionElement = connectionsElement.Element("SignalConnection")!;
    UpdateSignalConnectionElement(signalConnection, signalConnectionElement);
  }

  public virtual void UpdateMacroCcsScriptProcessor() {
    var connectionsElement = MacroCcsScriptProcessorElement!.Element("Connections");
    if (connectionsElement != null) {
      connectionsElement.RemoveAll();
    } else {
      connectionsElement = new XElement("Connections");
      MacroCcsScriptProcessorElement.Add(connectionsElement);
    }
    foreach (var signalConnection in ProgramConfig.MacroCcsScriptProcessor!.SignalConnections) {
      connectionsElement.Add(CreateSignalConnectionElement(signalConnection));
    }
  }
  
  private static void UpdateSignalConnectionElement(
    SignalConnection signalConnection, XElement signalConnectionElement) {
    var sourceAttribute = 
      signalConnectionElement.Attribute(nameof(SignalConnection.Source)) ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.{nameof(SignalConnection.Source)} attribute.");
    sourceAttribute.Value = signalConnection.Source;
    var destinationAttribute = 
      signalConnectionElement.Attribute(nameof(SignalConnection.Destination)) ??
      throw new ApplicationException(
        $"Cannot find {nameof(SignalConnection)}.{nameof(SignalConnection.Destination)} attribute.");
    destinationAttribute.Value = signalConnection.Destination;
  }
}