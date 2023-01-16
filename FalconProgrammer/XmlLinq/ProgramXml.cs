using System.Xml;
using System.Xml.Linq;
using FalconProgrammer.XmlDeserialised;
using JetBrains.Annotations;

namespace FalconProgrammer.XmlLinq;

public class ProgramXml {
  private XElement? _templateConstantModulationElement;
  private XElement? _templateRootElement;
  private XElement? _templateSignalConnectionElement;

  public ProgramXml(
    Category category, ScriptProcessor? infoPageCcsScriptProcessor = null) {
    Category = category;
    InfoPageCcsScriptProcessor = infoPageCcsScriptProcessor;
  }

  [PublicAPI] public Category Category { get; }
  private XElement ControlSignalSourcesElement { get; set; } = null!;
  private List<XElement> ConstantModulationElements { get; set; } = null!;
  [PublicAPI] public string InputProgramPath { get; set; } = null!;
  private ScriptProcessor? InfoPageCcsScriptProcessor { get; }
  protected XElement? InfoPageCcsScriptProcessorElement { get; private set; }
  private List<XElement> ModWheelSignalConnectionElements { get; set; } = null!;
  private XElement RootElement { get; set; } = null!;

  private XElement TemplateConstantModulationElement =>
    _templateConstantModulationElement ??= GetTemplateConstantModulationElement();

  private XElement TemplateRootElement =>
    _templateRootElement ??= XElement.Load(Category.TemplateProgramPath);

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

  public void AddMacro(ConstantModulation newMacro) {
    var constantModulationElement = new XElement(TemplateConstantModulationElement);
    var nameAttribute =
      constantModulationElement.Attribute(nameof(ConstantModulation.Name)) ??
      throw new ApplicationException(
        "Cannot find ConstantModulation.Name "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    nameAttribute.Value = newMacro.Name;
    var displayNameAttribute =
      constantModulationElement.Attribute(nameof(ConstantModulation.DisplayName)) ??
      throw new ApplicationException(
        "Cannot find ConstantModulation.DisplayName "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    displayNameAttribute.Value = newMacro.DisplayName;
    var bipolarAttribute =
      constantModulationElement.Attribute($"{nameof(ConstantModulation.Bipolar)}") ??
      throw new ApplicationException(
        "Cannot find ConstantModulation.Bipolar "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    bipolarAttribute.Value = newMacro.Bipolar.ToString();
    var styleAttribute =
      constantModulationElement.Attribute($"{nameof(ConstantModulation.Style)}") ??
      throw new ApplicationException(
        "Cannot find ConstantModulation.Style "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    styleAttribute.Value = newMacro.Style.ToString();
    var valueAttribute =
      constantModulationElement.Attribute($"{nameof(ConstantModulation.Value)}") ??
      throw new ApplicationException(
        "Cannot find ConstantModulation.Value "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    valueAttribute.Value = newMacro.Value.ToString();
    ControlSignalSourcesElement.Add(constantModulationElement);
    var signalConnectionElement =
      constantModulationElement.Descendants($"{nameof(SignalConnection)}")
        .FirstOrDefault();
    if (signalConnectionElement == null) {
      throw new ApplicationException(
        "Cannot find ConstantModulation.SignalConnection "
        + $"element in '{Category.TemplateProgramPath}'.");
    }
    UpdateSignalConnectionElement(newMacro.SignalConnections[0], signalConnectionElement);
    var propertiesElement = constantModulationElement.Element("Properties");
    if (propertiesElement == null) {
      throw new ApplicationException(
        "Cannot find ConstantModulation.Properties "
        + $"element in '{Category.TemplateProgramPath}'.");
    }
    var xAttribute =
      propertiesElement.Attribute("x") ??
      throw new ApplicationException(
        "Cannot find Properties.X "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    xAttribute.Value = newMacro.Properties.X.ToString();
    var yAttribute =
      propertiesElement.Attribute("y") ??
      throw new ApplicationException(
        "Cannot find Properties.Y "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    yAttribute.Value = newMacro.Properties.Y.ToString();
  }

  private bool ChangeConstantModulationValueToZero(string displayName) {
    var delayConstantModulationElement = (
      from constantModulationElement in ConstantModulationElements
      where string.Equals(constantModulationElement.Attribute("DisplayName")!.Value,
        displayName, StringComparison.OrdinalIgnoreCase)
      select constantModulationElement).FirstOrDefault();
    if (delayConstantModulationElement != null) {
      delayConstantModulationElement.Attribute("Value")!.Value = "0";
      return true;
    }
    return false;
  }

  public bool ChangeDelayConstantModulationValueToZero() {
    return ChangeConstantModulationValueToZero("Delay");
  }

  public bool ChangeReverbConstantModulationValueToZero() {
    return ChangeConstantModulationValueToZero("Reverb") ||
           ChangeConstantModulationValueToZero("Room") ||
           ChangeConstantModulationValueToZero("SparkVerb");
  }

  public void ChangeModWheelSignalConnectionSourcesToMacro(int macroNo) {
    string newSource = $"$Program/Macro {macroNo}";
    foreach (var signalConnectionElement in ModWheelSignalConnectionElements) {
      signalConnectionElement.Attribute("Source")!.Value = newSource;
    }
  }

  public void ChangeSignalConnectionSource(
    SignalConnection oldSignalConnection, SignalConnection newSignalConnection) {
    var signalConnectionElements =
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      where signalConnectionElement.Attribute("Source")!.Value ==
            oldSignalConnection.Source
      select signalConnectionElement;
    foreach (var signalConnectionElement in signalConnectionElements) {
      signalConnectionElement.Attribute("Source")!.Value = newSignalConnection.Source;
    }
  }

  private XElement CreateSignalConnectionElement(SignalConnection signalConnection) {
    var result = new XElement(TemplateSignalConnectionElement);
    // In case the template SignalConnection contains a non-default (< 1) Ratio,
    // set the Ratio to the default, 1.
    var ratioAttribute =
      result.Attribute(nameof(SignalConnection.Ratio)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Ratio "
        + $"attribute in '{Category.TemplateProgramPath}'.");
    ratioAttribute.Value = "1";
    UpdateSignalConnectionElement(signalConnection, result);
    return result;
  }

  public bool FindModWheelSignalConnections() {
    ModWheelSignalConnectionElements = (
      from signalConnectionElement in RootElement.Descendants("SignalConnection")
      where signalConnectionElement.Attribute("Source")!.Value == "@MIDI CC 1"
      select signalConnectionElement).ToList();
    return ModWheelSignalConnectionElements.Count > 0;
  }

  public void LoadFromFile(string inputProgramPath) {
    InputProgramPath = inputProgramPath;
    try {
      RootElement = XElement.Load(InputProgramPath);
      ControlSignalSourcesElement =
        RootElement.Descendants("ControlSignalSources").FirstOrDefault() ??
        throw new ApplicationException(
          $"Cannot find ControlSignalSources element in '{Category.TemplateProgramPath}'.");
      ConstantModulationElements = ControlSignalSourcesElement.Elements(
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

  private XElement GetTemplateConstantModulationElement() {
    var result =
      TemplateRootElement.Descendants("ConstantModulation").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find ConstantModulation element in '{Category.TemplateProgramPath}'.");
    return result;
  }

  protected virtual XElement GetTemplateSignalConnectionElement() {
    var result =
      TemplateRootElement.Descendants("SignalConnection").FirstOrDefault() ??
      throw new ApplicationException(
        $"Cannot find SignalConnection element in '{Category.TemplateProgramPath}'.");
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
        "Cannot find SignalConnection.Ratio "
        + $"attribute in '{InputProgramPath}'.");
    ratioAttribute.Value = signalConnection.Ratio.ToString();
    var sourceAttribute =
      signalConnectionElement.Attribute(nameof(SignalConnection.Source)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Source "
        + $"attribute in '{InputProgramPath}'.");
    sourceAttribute.Value = signalConnection.Source;
    var destinationAttribute =
      signalConnectionElement.Attribute(nameof(SignalConnection.Destination)) ??
      throw new ApplicationException(
        "Cannot find SignalConnection.Destination "
        + $"attribute in '{InputProgramPath}'.");
    destinationAttribute.Value = signalConnection.Destination;
  }
}